using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
public static class BuildMineDefense
{
 public static void Inspect()
 {
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");
  foreach(var root in scene.GetRootGameObjects())
  {
   var rr=root.GetComponentsInChildren<Renderer>();
   if(rr.Length==0)continue;
   Bounds b=rr[0].bounds; foreach(var r in rr)b.Encapsulate(r.bounds);
   Debug.Log("MERGE BOUNDS "+root.name+" "+b);
  }
  foreach(var c in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
   if(Vector3.Distance(c.bounds.center,new Vector3(63.63f,1.23f,-65.508f))<12f)
    Debug.Log("MERGE NEAR "+c.name+" "+c.bounds);
 }

 [MenuItem("Tools/MP2B/Create Combined Mine Defense Scene")]
 public static void Build()
 {
  const string output="Assets/Scenes/MineDefense.unity";
  if(System.IO.File.Exists(output)) throw new System.InvalidOperationException("MineDefense already exists; preserve your edits instead of rebuilding it.");
  if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
  var mine=EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity",OpenSceneMode.Single);
  // Save a new scene before moving anything. Originals remain untouched.
  EditorSceneManager.SaveScene(mine,output);
  var caveGroup=new GameObject("Drape_Cave").transform;
  foreach(var r in mine.GetRootGameObjects()) if(r!=caveGroup.gameObject)r.transform.SetParent(caveGroup,true);
  var rig=caveGroup.GetComponentInChildren<XROrigin>(true);
  var bank=caveGroup.GetComponentInChildren<EconomyManager>(true);
  if(rig==null || bank==null)throw new System.InvalidOperationException("Cave rig or economy missing.");
  var td=EditorSceneManager.OpenScene("Assets/Scenes/RoomScaleDefense.unity",OpenSceneMode.Additive);
  SceneManager.SetActiveScene(mine);
  var roomGroup=new GameObject("Billy_DefenseRoom").transform;
  // Move as one hierarchy so all scene-object references remain connected.
  var transfer=new GameObject("Transfer"); SceneManager.MoveGameObjectToScene(transfer,td);
  foreach(var r in td.GetRootGameObjects())if(r!=transfer)r.transform.SetParent(transfer.transform,true);
  foreach(var old in transfer.GetComponentsInChildren<XROrigin>(true))Object.DestroyImmediate(old.gameObject);
  foreach(var e in transfer.GetComponentsInChildren<EventSystem>(true))Object.DestroyImmediate(e.gameObject);
  if(caveGroup.GetComponentInChildren<XRInteractionManager>(true)!=null)
   foreach(var m in transfer.GetComponentsInChildren<XRInteractionManager>(true))Object.DestroyImmediate(m);
  foreach(var camera in transfer.GetComponentsInChildren<Camera>(true)) {camera.enabled=false;camera.tag="Untagged";}
  foreach(var audio in transfer.GetComponentsInChildren<AudioListener>(true))audio.enabled=false;
  foreach(var v in transfer.GetComponentsInChildren<UnityEngine.Rendering.Volume>(true))if(v.isGlobal)v.enabled=false;
  SceneManager.MoveGameObjectToScene(transfer,mine);
  foreach(Transform child in transfer.transform.Cast<Transform>().ToArray()) child.SetParent(roomGroup,true);
  Object.DestroyImmediate(transfer);
  EditorSceneManager.CloseScene(td,true);
  foreach(var resources in roomGroup.GetComponentsInChildren<PlayerResources>(true)) resources.sharedEconomy=bank;
  foreach(var input in roomGroup.GetComponentsInChildren<DesktopBuildInput>(true))input.viewCamera=rig.Camera;
  foreach(var bar in roomGroup.GetComponentsInChildren<EnemyHealthBar>(true))bar.viewCamera=rig.Camera;
  foreach(var lid in roomGroup.GetComponentsInChildren<DumpsterLid>(true))lid.playerHead=rig.Camera.transform;
  foreach(var canvas in roomGroup.GetComponentsInChildren<Canvas>(true))if(canvas.worldCamera!=null)canvas.worldCamera=rig.Camera;
  var front=roomGroup.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="BackWall (1)");
  // Replace the center of the front wall with a three-metre doorway.
  for(int i=0;i<2;i++)
  {
   var side=Object.Instantiate(front.gameObject,front.parent).transform;side.name="EntranceWall_"+i;
   side.localPosition=front.localPosition+Vector3.right*(i==0?-5.75f:5.75f);
   side.localScale=new Vector3(8.5f,front.localScale.y,front.localScale.z);
  }
  front.gameObject.SetActive(false);
  foreach(var t in roomGroup.GetComponentsInChildren<Transform>(true))if(t.name=="FrontBoundary_ColliderOnly")t.gameObject.SetActive(false);
  roomGroup.position=new Vector3(63f,0.08f,-43.8f);
  var passage=new GameObject("ConnectingPassage").transform;
  Material mat=roomGroup.GetComponentsInChildren<Renderer>().First(r=>r.name=="Floor").sharedMaterial;
  Block("PassageFloor",passage,new Vector3(63f,-0.02f,-52.65f),new Vector3(3f,0.2f,6.1f),mat);
  Block("PassageLeft",passage,new Vector3(61.4f,1.5f,-52.2f),new Vector3(.2f,3f,4.5f),mat);
  Block("PassageRight",passage,new Vector3(64.6f,1.5f,-52.2f),new Vector3(.2f,3f,4.5f),mat);
  // Open only the two boundary wall pieces at this connection, in this new scene.
  foreach(var t in caveGroup.GetComponentsInChildren<Transform>(true))
   if(t.name=="Wall_Mines_B (2)" || t.name=="Wall_Mines_B (3)" || t.name=="Panel_Wood_A (14)" || t.name=="Cave_Platform_A_Side (4)")t.gameObject.SetActive(false);
  Sign("MineDirection",passage,new Vector3(63f,2.5f,-49.5f),180f,"MINE  >  SELL ORE  >  BUILD DEFENSES");
  Sign("RoomDirection",passage,new Vector3(63f,2.5f,-55f),0f,"DEFENSE WORKSHOP\nOre sales fund towers and upgrades");
  foreach(var label in roomGroup.GetComponentsInChildren<TMP_Text>(true))label.text=label.text.Replace("SCRAP","GOLD");
  rig.Camera.enabled=true;rig.Camera.tag="MainCamera";
  Physics.SyncTransforms();
  Validate(mine);
  EditorSceneManager.SaveScene(mine,output);
  Debug.Log("MERGE SUCCESS: "+output);
 }
 private static void Block(string name,Transform parent,Vector3 pos,Vector3 scale,Material mat)
 {
  var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent);
  go.transform.position=pos;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=mat;
 }
 private static void Sign(string name,Transform parent,Vector3 position,float yaw,string text)
 {
  var label=new GameObject(name).AddComponent<TextMeshPro>();label.transform.SetParent(parent);
  label.transform.SetPositionAndRotation(position,Quaternion.Euler(0f,yaw,0f));
  label.text=text;label.fontSize=1.7f;label.alignment=TextAlignmentOptions.Center;label.rectTransform.sizeDelta=new Vector2(4f,1f);
 }
 public static void FinishPassage()
 {
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/MineDefense.unity");
  foreach(var t in scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)))
   if(t.name=="FrontBoundary_ColliderOnly" || t.name=="Panel_Wood_A (14)" || t.name=="Cave_Platform_A_Side (4)") t.gameObject.SetActive(false);
  Physics.SyncTransforms();Validate(scene);EditorSceneManager.SaveScene(scene);
 }
 public static void ValidateSaved()
 {
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/MineDefense.unity");Physics.SyncTransforms();Validate(scene);
 }
 private static void Validate(Scene scene)
 {
  var roots=scene.GetRootGameObjects();
  System.Action<bool,string> check=(ok,message)=>{if(!ok)throw new System.InvalidOperationException(message);};
  check(roots.SelectMany(r=>r.GetComponentsInChildren<XROrigin>(true)).Count()==1,"Expected one XR rig");
  check(roots.SelectMany(r=>r.GetComponentsInChildren<EventSystem>(true)).Count()==1,"Expected one EventSystem");
  check(roots.SelectMany(r=>r.GetComponentsInChildren<EconomyManager>(true)).Count()==1,"Expected one economy");
  var wallet=roots.SelectMany(r=>r.GetComponentsInChildren<PlayerResources>(true)).Single();
  check(wallet.sharedEconomy!=null,"Missing shared wallet reference");
  int initial=wallet.gold;wallet.AddGold(20);check(wallet.gold==initial+20,"Shared income failed");
  check(wallet.TrySpendGold(15)&&wallet.gold==initial+5,"Shared spending failed");
  check(!wallet.TrySpendGold(initial+6),"Overspending allowed");wallet.TrySpendGold(5);
  var waves=roots.SelectMany(r=>r.GetComponentsInChildren<SimpleWaveController>(true)).Single();
  check(waves.waypoints.Length>=2 && waves.waypoints.All(w=>w!=null),"Broken enemy path");
  check(roots.SelectMany(r=>r.GetComponentsInChildren<TowerDisplayShelf>(true)).Count(s=>s.towerPrefab!=null)==5,"Expected five stocked shelves");
  foreach(var t in roots.SelectMany(r=>r.GetComponentsInChildren<Transform>(true)))
   check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0,"Missing script: "+t.name);
  for(float x=62.2f;x<=63.8f;x+=.8f)
  for(float z=-55.5f;z<=-48.8f;z+=.4f)
  {
   var hits=Physics.OverlapCapsule(new Vector3(x,.5f,z),new Vector3(x,1.6f,z),.3f,~0,QueryTriggerInteraction.Ignore);
   foreach(var hit in hits)check(hit is CharacterController,"PASSAGE BLOCKER "+hit.name+" at "+x+", "+z+" bounds "+hit.bounds);
   check(Physics.Raycast(new Vector3(x,1f,z),Vector3.down,2f,~0,QueryTriggerInteraction.Ignore),"Passage floor gap "+z);
  }
  Debug.Log("MERGE VALIDATION PASSED: rig, economy, shelves, path, scripts, shared income/spending and floor.");
 }
}
