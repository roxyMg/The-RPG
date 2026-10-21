#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace HarborV2
{
    // Editor-only runner. Not serialized into the playable scene.
    public sealed class HarborPlaytest : MonoBehaviour
    {
        private HarborQuest quest;
        private Keyboard keyboard;
        private Mouse mouse;
        private readonly List<string> checks=new List<string>();
        private readonly List<string> errors=new List<string>();
        private bool failed,finished;
        private float deadline;
        private Player Player=>quest.player;
        private void Log(string text,string trace,LogType type)
        {if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(text+"\n"+trace);}
        private void Check(bool condition,string label)
        {checks.Add((condition?"PASS ":"FAIL ")+label);if(!condition)failed=true;Debug.Log(checks[checks.Count-1]);}
        private void Keys(params Key[] keys)=>InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));
        private IEnumerator Tap(Key key)
        {Keys();yield return null;Keys(key);yield return null;yield return null;Keys();yield return null;}
        private IEnumerator Walk(float x,float timeout=14)
        {
            float until=Time.time+timeout;
            while(Mathf.Abs(Player.transform.position.x-x)>.3f&&Time.time<until)
            {Keys(Player.transform.position.x<x?Key.D:Key.A);yield return new WaitForFixedUpdate();}
            Keys();yield return new WaitForSeconds(.2f);
            Check(Mathf.Abs(Player.transform.position.x-x)<.55f,"continuous walk to "+x+" actual="+Player.transform.position);
        }
        private IEnumerator JumpTo(float x)
        {
            float started=Time.time;
            Keys(Key.Space,Player.transform.position.x<x?Key.D:Key.A);yield return null;yield return null;
            while(Time.time-started<2&&(Time.time-started<.2f||!Player.groundDetected))
            {
                // The project keeps air inertia when movement is released: steer through real keys.
                Keys(Player.transform.position.x<x?Key.D:Key.A);yield return new WaitForFixedUpdate();
            }
            Keys();yield return new WaitForSeconds(.2f);
        }
        private void Teleport(Vector2 p)
        {Player.rb.position=p;Player.transform.position=p;Player.rb.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();}
        private HarborPoint Point(HarborPoint.Kind kind)=>quest.points.First(p=>p.kind==kind);

        private IEnumerator Start()
        {
            Debug.Log("HARBOR_V2_TEST_STARTED");Time.timeScale=1;
            Application.logMessageReceived+=Log;deadline=Time.realtimeSinceStartup+180;
            Application.runInBackground=true;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard=InputSystem.AddDevice<Keyboard>();keyboard.MakeCurrent();
            mouse=InputSystem.AddDevice<Mouse>();mouse.MakeCurrent();
            quest=FindAnyObjectByType<HarborQuest>();Camera.main.aspect=16f/9;
            yield return new WaitForSeconds(.7f);
            Check(Player.GetType()==typeof(Player),"uses original project Player, not a scene movement controller");
            Check(Player.moveSpeed==8&&Player.jumpForce==12&&Player.dashSpeed==20&&Player.dashDuration==.25f&&Player.rb.gravityScale==3.5f,"original movement jump dash gravity values");
            Check(Player.groundDetected,"original ground sensor works at spawn");
            Check(Player.anim.runtimeAnimatorController!=null&&Player.GetComponent<Player_Combat>()!=null&&Player.GetComponent<Player_Execution>()!=null,"original Animator combat execution installed");
            Capture("Start.png");
            Keys(Key.D);float runUntil=Time.time+1;float peakRunSpeed=0;
            while(Time.time<runUntil&&peakRunSpeed<7.7f){yield return new WaitForFixedUpdate();peakRunSpeed=Mathf.Max(peakRunSpeed,Player.rb.linearVelocity.x);}
            Check(Mathf.Abs(peakRunSpeed-8)<.3f,"original run speed physically reaches 8; observed="+peakRunSpeed);
            Keys();yield return new WaitForSeconds(.2f);
            float before=Player.transform.position.y;
            yield return Tap(Key.Space);yield return new WaitForSeconds(.15f);
            Check(Player.transform.position.y>before+.7f,"original Space jump moves body upward");
            yield return new WaitForSeconds(.8f);
            int ammo=Player.propulsionAmmo.CurrentAmmo;
            Keys(Key.LeftShift);yield return null;yield return null;
            Check(Mathf.Abs(Player.rb.linearVelocity.x)>18&&Player.propulsionAmmo.CurrentAmmo==ammo-1,"original Shift dash uses one propulsion round");
            Keys();yield return new WaitForSeconds(.4f);
            Check(Player.rb.gravityScale==3.5f,"dash restores original gravity");
            yield return Walk(11);yield return Tap(Key.Q);
            Check(quest.HasPermit,"Q obtains permit without occupying E execution binding");
            yield return Walk(33);yield return Tap(Key.Q);
            Check(quest.Checkpoint.x==33,"continuous downward ramp reaches inn checkpoint");
            Capture("Inn.png");
            yield return Walk(36.3f);
            var crate=FindAnyObjectByType<HarborCrate>();
            InputSystem.QueueStateEvent(mouse,new MouseState().WithButton(MouseButton.Left));yield return null;yield return null;
            Check(Player.attackState.IsActive,"original mouse attack enters original FSM attack state");
            InputSystem.QueueStateEvent(mouse,new MouseState());yield return new WaitForSeconds(1.1f);
            Check(crate.Hits>0&&crate.Health<30,"original animation event and damage hit cargo crate");
            Check(!Player.attackState.IsActive,"original attack animation returns to locomotion");
            int lethal=Player.lethalAmmo.CurrentAmmo;
            InputSystem.QueueStateEvent(mouse,new MouseState().WithButton(MouseButton.Right));yield return null;yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState());yield return new WaitForSeconds(.8f);
            Check(Player.lethalAmmo.CurrentAmmo==lethal-1,"original right mouse counter consumes original lethal ammo");
            yield return Walk(48);yield return Tap(Key.Q);
            Check(quest.GateOpen&&!quest.gateCollider.enabled,"winch removes gate collision");
            yield return Walk(62);Capture("Dock.png");yield return Tap(Key.Q);
            Check(quest.Complete&&Time.timeScale==0,"continuous main route ends in victory");
            yield return Tap(Key.R);yield return new WaitForSeconds(.4f);
            Check(!quest.Complete&&!quest.GateOpen&&!quest.HasPermit&&quest.gateCollider.enabled,"restart resets quest and gate");
            Check(FindObjectsByType<Player>().Length==1&&Player.propulsionAmmo.CurrentAmmo==3,"restart recreates exactly one original player and fresh ammo");
            // Reverse route traverses both uphill transitions with the unchanged motor.
            Teleport(new Vector2(50,1.3f));yield return new WaitForSeconds(.4f);
            yield return Walk(24);yield return Walk(6);
            Check(Player.groundDetected&&Player.transform.position.y>5,"original motor climbs both continuous slopes");
            Teleport(new Vector2(51.1f,1.3f));yield return new WaitForSeconds(.3f);
            Keys(Key.D);yield return new WaitForSeconds(.8f);Keys();yield return new WaitForSeconds(.1f);
            Check(Player.transform.position.x<52.1f,"closed gate physically blocks walking");
            Teleport(new Vector2(48,1.3f));yield return new WaitForSeconds(.2f);yield return Tap(Key.Q);
            Check(!quest.GateOpen,"no permit cannot unlock gate");
            Teleport(new Vector2(62,1.3f));yield return new WaitForSeconds(.2f);yield return Tap(Key.Q);
            Check(!quest.Complete,"no permit and gate cannot finish");
            Teleport(new Vector2(33,3.3f));yield return new WaitForSeconds(.2f);yield return Tap(Key.Q);
            Teleport(new Vector2(66,-6));yield return new WaitForSeconds(.4f);
            Check(quest.Falls==1&&Mathf.Abs(Player.transform.position.x-33)<.1f,"fall replaces player at checkpoint without stale FSM state");
            Player.GetComponent<Entity_Health>().TakeDamage(1000,null,0);yield return new WaitForSeconds(.5f);
            Check(quest.Falls==2&&!Player.GetComponent<Entity_Health>().IsDead,"death restores original healthy player");
            // Both optional platforms must be reachable using only the original jump and air control.
            Teleport(new Vector2(16,4.8f));yield return new WaitForSeconds(.4f);
            yield return JumpTo(18);
            Check(Player.groundDetected&&Player.transform.position.y>5.35f,"original jump reaches first optional platform");
            yield return JumpTo(20);
            Check(Player.groundDetected&&Player.transform.position.y>6.05f,"original jump reaches high relic platform");
            yield return Tap(Key.Q);Check(quest.Relics==1,"high relic collected through real jump route");
            foreach(var relic in quest.points.Where(p=>p.kind==HarborPoint.Kind.Relic))
            {
                Teleport(relic.transform.position);yield return new WaitForSeconds(.25f);
                quest.Interact(relic);quest.Interact(relic);
                Check(Player.groundDetected,"solid standing surface at "+relic.name);
            }
            Check(quest.Relics==3,"three relics count once");
            yield return Tap(Key.Escape);Check(quest.Paused&&Time.timeScale==0&&!Player.input.Player.enabled,"pause disables original input actions");
            yield return Tap(Key.Escape);Check(!quest.Paused&&Time.timeScale==1&&Player.input.Player.enabled,"resume restores original input actions");
            yield return Tap(Key.Escape);yield return Tap(Key.R);yield return new WaitForSeconds(.4f);
            Check(!quest.Paused&&Time.timeScale==1&&quest.Relics==0&&quest.Falls==0,"restart from pause resets full round");
            yield return Tap(Key.Tab);Check(quest.view.Overview,"Tab overview works");Capture("OverviewRuntime.png");
            yield return Tap(Key.Tab);Check(!quest.view.Overview,"Tab returns to follow");
            Check(errors.Count==0,"no runtime errors or exceptions");Finish();
        }
        private void Update(){if(!finished&&deadline>0&&Time.realtimeSinceStartup>deadline){Check(false,"test timeout");Finish();}}
        private void Capture(string filename)
        {
            Render(Camera.main,"Assets/Scenes/HarborVillageV2"+"/Preview/"+filename);
        }
        public static void Render(Camera camera,string path)
        {
            var rt=new RenderTexture(1600,900,24);var previous=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            var texture=new Texture2D(1600,900,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1600,900),0,0);texture.Apply();
            File.WriteAllBytes(path,texture.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=previous;rt.Release();Object.DestroyImmediate(texture);Object.DestroyImmediate(rt);
        }
        private void Finish()
        {
            if(finished)return;finished=true;SessionState.SetBool("HarborV2.TestRunning",false);StopAllCoroutines();Application.logMessageReceived-=Log;Time.timeScale=1;
            InputSystem.RemoveDevice(keyboard);InputSystem.RemoveDevice(mouse);
            File.WriteAllText("Assets/Scenes/HarborVillageV2"+"/Preview/Playtest.txt",(failed?"FAILED":"PASSED")+"\n"+string.Join("\n",checks)+"\nERRORS\n"+string.Join("\n",errors));
            EditorApplication.Exit(failed?1:0);
        }
    }
}
#endif
