var paths = new[] { "Assets/RingTossMaterials/RingShape.prefab", "Assets/RingTossTutorial/Meshes/RingPlaceholder.asset", "Assets/RingTossTutorial/Materials/Ring.mat" };
foreach (var p in paths) if (UnityEditor.AssetDatabase.LoadMainAssetAtPath(p) == null) throw new System.Exception("Missing asset: " + p);
var output = "Docs/CourseValidation/Distribution/RingShape_v2.unitypackage";
if (System.IO.File.Exists(output)) throw new System.Exception("Package already exists; refusing overwrite.");
System.IO.Directory.CreateDirectory("Docs/CourseValidation/Distribution");
UnityEditor.AssetDatabase.ExportPackage(paths, output, UnityEditor.ExportPackageOptions.Default);
return new { output, paths, bytes = new System.IO.FileInfo(output).Length };
