#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace CombatTest
{
    // Explicit opt-in batch regression runner. Excluded from every standalone build.
    public sealed class CombatTestSmokeProbe : MonoBehaviour
    {
        private readonly List<string> checks = new List<string>();
        private readonly List<string> errors = new List<string>();
        private CombatTestRoom room;
        private float deadline;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(),"-combatSmoke") >= 0)
                new GameObject("Automated combat regression (editor only)").AddComponent<CombatTestSmokeProbe>();
        }
        private void Awake()
        {
            deadline = Time.realtimeSinceStartup + 100;
            Application.logMessageReceived += Log;
        }
        private void Log(string message,string trace,LogType kind)
        {
            if (kind == LogType.Exception || kind == LogType.Error || kind == LogType.Assert)
                errors.Add(message + "\n" + trace);
        }
        private void Update()
        {
            if (Time.realtimeSinceStartup > deadline) Finish("TIMEOUT");
        }
        private void Check(bool condition,string label)
        {
            if (!condition) { Finish("FAILED: " + label); throw new Exception(label); }
            checks.Add("PASS " + label);
            Debug.Log("COMBAT_CHECK " + label);
        }
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(.8f);
            room = FindFirstObjectByType<CombatTestRoom>();
            Check(room != null && room.Player != null && room.Elite != null,"room spawns both actors");
            Check(room.PlayerHealth.CurrentHealth == 100 && room.EliteHealth.CurrentHealth == 240,"initial HP 100 / 240");
            Check(room.Player.groundDetected && room.Elite.groundDetected,"both actors stand on floor");
            room.Player.rb.position = new Vector2(room.Elite.transform.position.x - 1.7f,1.3f);
            room.Player.HandFlip(1);
            // Batchmode has no focused game window. Exercise the real FSM/animation;
            // physical keyboard/mouse routing is checked in the standalone window.
            var machine = (StateMachine)typeof(Entity).GetField("stateMachine",BindingFlags.Instance | BindingFlags.NonPublic).GetValue(room.Player);
            machine.ChangeState(room.Player.attackState);
            yield return new WaitForSeconds(.12f);
            Check(room.Player.attackState.IsActive,"original player attack state runs");
            yield return new WaitForSeconds(1);
            Check(room.EliteHealth.CurrentHealth < 240,"original melee animation event damages elite");
            Check(!room.Player.attackState.IsActive,"original melee animation returns to movement");
            yield return Reset();
            for (int move = 0; move < 3; move++)
            {
                yield return WaitPhase(Enemy_TestElite.Phase.Windup,(Enemy_TestElite.Move)move);
                Check(room.Elite.CanBeCountered == (move != 2),"counter window move " + move);
                var bounds = room.Elite.HitBounds(true);
                room.Player.rb.position = new Vector2(move == 1 ? room.Elite.transform.position.x + room.Elite.facingDirection * 2 : bounds.center.x,1.3f);
                room.Player.rb.linearVelocity = Vector2.zero;
                float before = room.PlayerHealth.CurrentHealth;
                yield return WaitPhase(Enemy_TestElite.Phase.Recovery,(Enemy_TestElite.Move)move);
                float expected = move == 0 ? 12 : move == 1 ? 16 : 22;
                Check(Mathf.Approximately(before-room.PlayerHealth.CurrentHealth,expected),"move " + move + " damages once: " + expected);
                Check(!room.Elite.CanBeCountered,"counter window closes after move " + move);
            }
            yield return Reset();
            yield return WaitPhase(Enemy_TestElite.Phase.Windup,Enemy_TestElite.Move.Cleave);
            room.Player.rb.position = new Vector2(room.Elite.transform.position.x + room.Elite.facingDirection * 1.7f,1.3f);
            room.Player.HandFlip(-room.Elite.facingDirection);
            Physics2D.SyncTransforms();
            Check(room.Player.GetComponent<Player_Combat>().CounterAttackPerformed(),"real player counter query connects");
            Check(room.Elite.IsStunned,"counter interrupts elite attack");
            yield return new WaitForSeconds(.3f);
            room.Player.rb.position = new Vector2(room.Elite.transform.position.x + room.Elite.facingDirection * 1.7f,1.3f);
            Physics2D.SyncTransforms();
            Check(room.Player.TryStartExecution(),"player starts execution on stunned elite");
            yield return new WaitForSeconds(.1f);
            Check(room.Elite.IsExecutionLocked,"execution reserves elite");
            float beforeExecution = room.EliteHealth.CurrentHealth;
            yield return new WaitForSeconds(.85f);
            Check(Mathf.Approximately(beforeExecution-room.EliteHealth.CurrentHealth,25),"execution settles configured damage once");
            Check(!room.Elite.IsExecutionLocked && !room.Player.execution.IsExecuting,"execution finishes and releases both actors");
            yield return Reset();
            yield return WaitPhase(Enemy_TestElite.Phase.Windup,Enemy_TestElite.Move.Cleave);
            room.Player.rb.position = new Vector2(room.Elite.transform.position.x + room.Elite.facingDirection * 1.7f,1.3f);
            room.Player.HandFlip(-room.Elite.facingDirection);
            Physics2D.SyncTransforms();
            room.Player.GetComponent<Player_Combat>().CounterAttackPerformed();
            yield return new WaitForSeconds(.3f);
            room.Player.rb.position = new Vector2(room.Elite.transform.position.x + room.Elite.facingDirection * 1.7f,1.3f);
            Physics2D.SyncTransforms();
            Check(room.Player.TryStartExecution(),"prepare execution interruption retry");
            yield return Reset();
            Check(!room.Player.execution.IsExecuting && !room.Elite.IsExecutionLocked,"restart during execution releases locks");
            room.Player.invulnerability.BeginInvulnerability(.3f);
            room.PlayerHealth.TakeDamage(25,room.Elite.transform,0);
            Check(room.PlayerHealth.CurrentHealth == 100,"existing invulnerability blocks damage");
            yield return new WaitForSeconds(.35f);
            room.PlayerHealth.TakeDamage(1000,room.Elite.transform,0);
            yield return null;
            Check(room.Result != null && room.Result.Contains("DEFEAT"),"player death shows defeat");
            int count = room.Elite.AttacksStarted;
            yield return new WaitForSeconds(1);
            Check(room.Elite.AttacksStarted == count,"elite stops attacking dead player");
            yield return Reset();
            room.EliteHealth.TakeDamage(1000,room.Player.transform,0);
            yield return new WaitForSeconds(1.5f);
            Check(room.Result != null && room.Result.Contains("VICTORY"),"elite death shows victory after corpse cleanup");
            for (int i=0;i<3;i++)
            {
                yield return Reset();
                room.Player.lethalAmmo.TryConsume();
                room.Player.propulsionAmmo.TryConsume();
                room.Player.GetComponent<Player_ReloadProgress>().AddProgress(25);
            }
            yield return Reset();
            room.TogglePause();
            Check(Time.timeScale == 0,"pause freezes clock");
            yield return Reset();
            Check(Time.timeScale == 1 && !room.Paused,"restart from pause resumes clock");
            Check(errors.Count == 0,"no runtime errors or exceptions");
            Finish("PASSED");
        }
        private IEnumerator WaitPhase(Enemy_TestElite.Phase phase,Enemy_TestElite.Move move)
        {
            float until = Time.time + 18;
            while (room.Elite != null && !(room.Elite.CurrentPhase == phase && room.Elite.CurrentMove == move))
            {
                if (Time.time > until) { Check(false,"reach phase " + phase + " / " + move); yield break; }
                yield return null;
            }
        }
        private IEnumerator Reset()
        {
            int round = room.Round;
            room.RestartRound();
            yield return null;
            yield return new WaitForSeconds(.15f);
            Check(room.Round == round + 1,"restart advances exactly one round");
            Check(FindObjectsByType<Player>(FindObjectsSortMode.None).Length == 1 && FindObjectsByType<Enemy_TestElite>(FindObjectsSortMode.None).Length == 1,"restart leaves one actor of each kind");
            Check(room.PlayerHealth.CurrentHealth == 100 && room.EliteHealth.CurrentHealth == 240,"restart restores both health pools");
            Check(room.Player.lethalAmmo.CurrentAmmo == 3 && room.Player.propulsionAmmo.CurrentAmmo == 3 && room.Player.GetComponent<Player_ReloadProgress>().ReloadProgress == 0,"restart resets ammunition and reload progress");
        }
        private void Finish(string result)
        {
            Application.logMessageReceived -= Log;
            Directory.CreateDirectory("D:/BaiduNetdiskDownload/CombatTestRoom_v0.1/QA");
            File.WriteAllText("D:/BaiduNetdiskDownload/CombatTestRoom_v0.1/QA/smoke-results.txt", result + "\n" + string.Join("\n",checks) + "\nERRORS\n" + string.Join("\n",errors));
            enabled = false;
            int exitCode = result == "PASSED" ? 0 : 1;
            EditorApplication.delayCall += () =>
            {
                EditorApplication.isPlaying = false;
                EditorApplication.delayCall += () => EditorApplication.Exit(exitCode);
            };
        }
    }
}
#endif
