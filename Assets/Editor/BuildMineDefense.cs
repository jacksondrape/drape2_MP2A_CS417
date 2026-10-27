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
 [MenuItem("Tools/MP2B/Restore Missing Cave Geometry %&u")]
 public static void RestoreCaveGeometry()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode)return;
  var scene=SceneManager.GetActiveScene();
  if(scene.path!="Assets/Scenes/MineDefense.unity")return;
  var mine=scene.GetRootGameObjects().Single(r=>r.name=="Drape_Cave").transform.Find("MinePlayspace");
  var source=EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity",OpenSceneMode.Additive);
  var original=source.GetRootGameObjects().Single(r=>r.name=="MinePlayspace").transform;
  SceneManager.SetActiveScene(scene);
  var restored=new System.Collections.Generic.List<string>();
  try
  {
   foreach(Transform child in original)
   {
    if(mine.Find(child.name)!=null)continue;
    if(child.GetComponent<Renderer>()==null)continue;
    var copy=Object.Instantiate(child.gameObject,mine,false);copy.name=child.name;
    Undo.RegisterCreatedObjectUndo(copy,"Restore cave geometry");restored.Add(child.name);
   }
  }
  finally{EditorSceneManager.CloseScene(source,true);}
  EditorSceneManager.MarkSceneDirty(scene);
  InspectConnection();
  System.IO.File.WriteAllLines("RestoredCaveGeometry.txt",restored);
  Debug.Log("Restored cave pieces: "+restored.Count+". Not yet saved; checking connection first.");
 }
 [MenuItem("Tools/MP2B/Align Room and Cave %&j")]
 public static void AlignConnection()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode)return;
  var scene=SceneManager.GetActiveScene();
  if(scene.path!="Assets/Scenes/MineDefense.unity")throw new System.InvalidOperationException("Open MineDefense first.");
  System.IO.Directory.CreateDirectory("SceneBackups");
  System.IO.File.Copy(scene.path,"SceneBackups/MineDefense-before-alignment-"+System.DateTime.Now.ToString("yyyyMMdd-HHmmss")+".unity");
  var room=scene.GetRootGameObjects().Single(r=>r.name=="Billy_DefenseRoom").transform;
  var passage=scene.GetRootGameObjects().Single(r=>r.name=="ConnectingPassage").transform;
  Undo.RecordObject(room,"Align defense room");
  room.position=new Vector3(78f,.08f,-37.8f);
  var parts=room.GetComponentsInChildren<Transform>(true);
  for(int i=0;i<2;i++)
  {
   var wall=parts.Single(t=>t.name=="EntranceWall_"+i);
   Undo.RecordObject(wall,"Align doorway");
   wall.localPosition=new Vector3(i==0?-5.5f:5.5f,wall.localPosition.y,wall.localPosition.z);
   wall.localScale=new Vector3(9f,wall.localScale.y,wall.localScale.z);
  }
  var mat=parts.Single(t=>t.name=="Floor").GetComponent<Renderer>().sharedMaterial;
  ConnectionBlock("PassageFloor",passage,new Vector3(64f,-.02f,-53.8125f),new Vector3(2f,.2f,1.625f),mat);
  ConnectionBlock("PassageLeft",passage,new Vector3(62.85f,1.58f,-52.7375f),new Vector3(.3f,3.2f,3.775f),mat);
  ConnectionBlock("PassageRight",passage,new Vector3(65.15f,1.58f,-53.8125f),new Vector3(.3f,3.2f,1.625f),mat);
  ConnectionBlock("PassageCeiling",passage,new Vector3(64f,3.23f,-53.8125f),new Vector3(2.6f,.2f,1.625f),mat);
  ConnectionBlock("CrossFloor",passage,new Vector3(71f,-.02f,-52f),new Vector3(16f,.2f,2f),mat);
  ConnectionBlock("CrossCeiling",passage,new Vector3(71f,3.23f,-52f),new Vector3(16.6f,.2f,2f),mat);
  ConnectionBlock("CrossSouth",passage,new Vector3(72.15f,1.58f,-53.15f),new Vector3(14.3f,3.2f,.3f),mat);
  ConnectionBlock("CrossNorth",passage,new Vector3(69.85f,1.58f,-50.85f),new Vector3(14.3f,3.2f,.3f),mat);
  ConnectionBlock("RoomLegFloor",passage,new Vector3(78f,-.02f,-47.3875f),new Vector3(2f,.2f,7.225f),mat);
  ConnectionBlock("RoomLegCeiling",passage,new Vector3(78f,3.23f,-47.3875f),new Vector3(2.6f,.2f,7.225f),mat);
  ConnectionBlock("RoomLegLeft",passage,new Vector3(76.85f,1.58f,-47.3875f),new Vector3(.3f,3.2f,7.225f),mat);
  ConnectionBlock("RoomLegRight",passage,new Vector3(79.15f,1.58f,-48.3875f),new Vector3(.3f,3.2f,9.225f),mat);
  // Close the sides of the old wide cave opening, leaving the aligned passage free.
  ConnectionBlock("CaveDoorLeft",passage,new Vector3(62.8f,1.65f,-54.25f),new Vector3(.4f,3.3f,.5f),mat);
  ConnectionBlock("CaveDoorRight",passage,new Vector3(66.8f,1.65f,-54.25f),new Vector3(3.6f,3.3f,.5f),mat);
  ConnectionBlock("CaveDoorHeader",passage,new Vector3(65.6f,3.75f,-54.25f),new Vector3(6f,1.25f,.5f),mat);
  ConnectionBlock("RoomDoorHeader",passage,new Vector3(78f,5f,-43.82f),new Vector3(2f,3.76f,.15f),mat);
  var lights=new[]{new Vector3(64,2.7f,-52),new Vector3(68,2.7f,-52),new Vector3(73,2.7f,-52),new Vector3(78,2.7f,-51),new Vector3(78,2.7f,-46)};
  for(int i=0;i<lights.Length;i++)
  {
   string name="PassageLight_"+i;var lamp=passage.Find(name);
   if(lamp==null){lamp=new GameObject(name).transform;lamp.SetParent(passage);lamp.gameObject.AddComponent<Light>();Undo.RegisterCreatedObjectUndo(lamp.gameObject,"Light passage");}
   lamp.position=lights[i];
   var light=lamp.GetComponent<Light>();light.type=LightType.Point;light.color=new Color(1f,.77f,.48f);light.range=6f;light.intensity=2f;
  }
  passage.Find("MineDirection").position=new Vector3(78f,2.7f,-43.6f);
  passage.Find("RoomDirection").position=new Vector3(64f,2.7f,-54.6f);
  foreach(var label in passage.GetComponentsInChildren<TMP_Text>()) {label.fontSize=1f;label.rectTransform.sizeDelta=new Vector2(1.9f,.6f);}
  Physics.SyncTransforms();
  var results=new System.Collections.Generic.List<string>();
  var route=new System.Collections.Generic.List<Vector3>();
  for(float lane=-.6f;lane<=.61f;lane+=.6f)
  {
   for(float z=-55;z<=-52;z+=.25f)route.Add(new Vector3(64+lane,0,z));
   for(float x=64;x<=78;x+=.25f)route.Add(new Vector3(x,0,-52+lane));
   for(float z=-52;z<=-43;z+=.25f)route.Add(new Vector3(78+lane,0,z));
  }
  foreach(var point in route)
  {
   float x=point.x,z=point.z;
   foreach(var c in Physics.OverlapCapsule(new Vector3(x,.5f,z),new Vector3(x,1.8f,z),.25f,~0,QueryTriggerInteraction.Ignore))
    if(!(c is CharacterController))results.Add("BLOCKED "+c.name+" "+x+", "+z);
   if(!Physics.Raycast(new Vector3(x,.4f,z),Vector3.down,.6f,~0,QueryTriggerInteraction.Ignore))results.Add("FLOOR GAP "+x+", "+z);
  }
  System.IO.File.WriteAllLines("ConnectionValidation.txt",results.Count==0?new[]{"PASS: "+route.Count+" floor and capsule samples across three lanes."}:results.ToArray());
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
  SceneView.lastActiveSceneView.LookAt(new Vector3(64f,1.6f,-48f),Quaternion.Euler(15f,180f,0f),9f);
  Debug.Log("Connection aligned. Validation issues: "+results.Count);
 }
 [MenuItem("Tools/MP2B/Preview Connection %&k")]
 public static void PreviewConnection()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode)return;
  var go=new GameObject("TemporaryConnectionCamera");var camera=go.AddComponent<Camera>();
  var rt=new RenderTexture(1000,700,24);camera.targetTexture=rt;camera.fieldOfView=70f;
  var old=RenderTexture.active;
  try
  {
   for(int i=0;i<4;i++)
   {
    go.transform.position=new Vector3(i==0?78f:64f,1.7f,i==0?-41f:-57f);
    go.transform.rotation=Quaternion.Euler(0,i==0?180f:0f,0);
    if(i==2){go.transform.position=new Vector3(56.6f,1.7f,-53f);go.transform.rotation=Quaternion.identity;}
    if(i==3){go.transform.position=new Vector3(61f,1.8f,-48f);go.transform.rotation=Quaternion.Euler(0,90,0);}
    camera.Render();RenderTexture.active=rt;var texture=new Texture2D(1000,700,TextureFormat.RGB24,false);
    texture.ReadPixels(new Rect(0,0,1000,700),0,0);texture.Apply();
    System.IO.File.WriteAllBytes("ConnectionView"+i+".png",texture.EncodeToPNG());Object.DestroyImmediate(texture);
   }
  }
  finally{RenderTexture.active=old;camera.targetTexture=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(go);}
 }
 private static void ConnectionBlock(string name,Transform parent,Vector3 position,Vector3 size,Material material)
 {
  var t=parent.Find(name);
  if(t==null){Block(name,parent,position,size,material);Undo.RegisterCreatedObjectUndo(parent.Find(name).gameObject,"Build passage");}
  else {Undo.RecordObject(t,"Align passage");t.position=position;t.localScale=size;}
 }
 [MenuItem("Tools/MP2B/Inspect Current Connection %&i")]
 public static void InspectConnection()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode)return;
  var scene=SceneManager.GetActiveScene();
  var lines=new System.Collections.Generic.List<string>();
  foreach(var t in scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)))
  {
   var r=t.GetComponent<Renderer>();var c=t.GetComponent<Collider>();
   if(r==null && c==null && t.parent!=null)continue;
   string path=t.name;for(var p=t.parent;p!=null;p=p.parent)path=p.name+"/"+path;
   lines.Add(path+" | active="+t.gameObject.activeInHierarchy+" pos="+t.position.ToString("F3")+" scale="+t.lossyScale.ToString("F3")
     +(r!=null?" bounds="+r.bounds.ToString("F3"):"")+(c!=null?" collider="+c.bounds.ToString("F3"):""));
  }
  System.IO.File.WriteAllLines("ConnectionInspection.txt",lines);
  Debug.Log("Connection inspection saved.");
 }
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
