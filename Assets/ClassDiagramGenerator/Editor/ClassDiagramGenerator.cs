using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ClassDiagramGenerator
{
    public class ClassDiagramGenerator : EditorWindow
    {
        private enum ExportFormat { PlantUMLFile, PlantUML_URL }
        private ExportFormat _exportFormat = ExportFormat.PlantUMLFile;
        private string _lastDiagramURL = "";
        private string _outputPath = "Assets/Scripts/ClassDiagram.puml";
        private string _status = "";
        private Vector2 _scroll;
        private bool _includeAssociations = true;

        // Nouveaux champs pour sélection des scripts
        private ScriptSelectionManager _scriptManager = new ScriptSelectionManager();
        private Vector2 _scriptScroll;
        private string _scriptSearch = "";
        private bool _scriptsScanned = false;

        private Color _mainBg = new Color(0.09f, 0.09f, 0.11f, 0.98f); // #17181C
        private Color _accent = new Color(0.82f, 0.14f, 1.0f, 1f);     // #D024FF 
        private Color _secondary = new Color(0.82f, 0.14f, 1.0f, 1f);  // #9C2FFF  
        private Color _hotPink = new Color(0.82f, 0.14f, 1.0f, 1f);     // #FF30B1  
        private Color _glowWhite = new Color(0.95f, 0.95f, 1f, 1f);    // quasi blanc
        private Color _activeTab = new Color(0.55f, 0.14f, 0.50f, 1f);  // Violet sombre
        private Color _inactiveTab = new Color(0.85f, 0.34f, 1.0f, 1f); // Violet plus clair/flashy

        private Texture2D GetHeaderIcon()
        {
            return Resources.Load<Texture2D>("Icon 160x160 - Diagram Generator");
        }

        [MenuItem("Tools/🧬 Diagram Generator")]
        static void ShowWindow()
        {
            GetWindow<ClassDiagramGenerator>("Diagram Generator").minSize = new Vector2(520, 340);
        }

        private void OnGUI()
        {
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), _mainBg);

            GUILayout.Space(10);

            var headerTex = GetHeaderIcon();

            if (headerTex != null)
            {
                float maxLogoWidth = Mathf.Min(128, position.width * 0.25f);
                float logoRatio = (float)headerTex.height / headerTex.width;
                float logoWidth = maxLogoWidth;
                float logoHeight = maxLogoWidth * logoRatio;

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(headerTex, GUILayout.Width(logoWidth), GUILayout.Height(logoHeight));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(2);
            }

            GUILayout.Space(16);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 28,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = _glowWhite }
            };
            var genStyle = new GUIStyle(titleStyle);
            genStyle.normal.textColor = _accent;

            float titleHeight = titleStyle.CalcHeight(new GUIContent("Diagram"), 400);

            Rect rect1 = GUILayoutUtility.GetRect(new GUIContent("Diagram "), titleStyle, GUILayout.Height(titleHeight));
            Rect rect2 = GUILayoutUtility.GetRect(new GUIContent("Generator"), genStyle, GUILayout.Height(titleHeight));

            GUI.Label(rect1, "Diagram ", titleStyle);
            GUI.Label(rect2, "Generator", genStyle);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            var subStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = _hotPink }
            };
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("FROM CODE TO CLASS DIAGRAM, INSTANTLY", subStyle, GUILayout.Width(330));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(16);

            DrawNeonSection(() =>
            {
                EditorGUILayout.HelpBox("Select which scripts to include below, then build a class diagram (PlantUML compatible).", MessageType.Info);
            });

            // *** Sélection des scripts à inclure ***
            DrawNeonSection(() =>
            {
                EditorGUILayout.LabelField("Scripts to include in the diagram :", EditorStyles.boldLabel);
                if (!_scriptsScanned)
                {
                    if (GUILayout.Button("🔍 Scan scripts"))
                    {
                        _scriptManager.Scan("Assets/Scripts");
                        _scriptsScanned = true;
                    }
                }
                else
                {
                    DrawScriptSelectionUI();
                    if (GUILayout.Button("↻ Rescan"))
                    {
                        _scriptManager.Scan("Assets/Scripts");
                    }
                }
            });

            DrawNeonSection(() =>
            {
                var labelStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = _accent }
                };
                EditorGUILayout.LabelField("Choose export format:", labelStyle);
                GUILayout.Space(8);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                var prev = GUI.backgroundColor;
                float btnWidth = Mathf.Max(160, position.width * 0.36f);
                for (int i = 0; i < 2; i++)
                {
                    GUI.backgroundColor = (int)_exportFormat == i ? _activeTab : _inactiveTab;
                    var btnText = i == 0 ? "📄 PlantUML File" : "🌐 PlantUML URL";
                    if (GUILayout.Button(btnText, GUILayout.Height(38), GUILayout.Width(btnWidth)))
                        _exportFormat = (ExportFormat)i;
                    if (i == 0) GUILayout.Space(14);
                }
                GUI.backgroundColor = prev;

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            });

            DrawNeonSection(() =>
            {
                var labelStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = _accent }
                };
                EditorGUILayout.LabelField("Advanced options:", labelStyle);
                _includeAssociations = EditorGUILayout.ToggleLeft("Include associations (fields/parameters of other classes)", _includeAssociations);
            });

            DrawNeonSection(() =>
            {
                var labelStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = _accent }
                };
                EditorGUILayout.LabelField("PlantUML output path:", labelStyle);
                _outputPath = EditorGUILayout.TextField(_outputPath);

                GUILayout.Space(10);

                DrawWideNeonButton("🛠️ Generate Diagram", () => GenerateDiagram(_exportFormat), _hotPink, 44);

                if (_exportFormat == ExportFormat.PlantUML_URL && !string.IsNullOrEmpty(_lastDiagramURL))
                {
                    GUILayout.Space(10);
                    EditorGUILayout.SelectableLabel(_lastDiagramURL, EditorStyles.textField, GUILayout.Height(22));
                    DrawWideNeonButton("📋 Copy URL", () => EditorGUIUtility.systemCopyBuffer = _lastDiagramURL, _accent, 32);
                }
            });

            GUILayout.FlexibleSpace();

            var statStyle = new GUIStyle(EditorStyles.helpBox)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                normal = { textColor = _glowWhite }
            };
            GUILayout.Space(2);
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(36));
            EditorGUILayout.LabelField(_status, statStyle);
            EditorGUILayout.EndScrollView();

            var sigStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                fontStyle = FontStyle.Italic,
                normal = { textColor = _secondary }
            };
            GUILayout.Label("© 2025 ClassDiagramGenerator • v1.2", sigStyle);

            GUILayout.Space(4);
        }

        private void DrawScriptSelectionUI()
        {
            GUILayout.BeginHorizontal();
            _scriptSearch = EditorGUILayout.TextField("Search :", _scriptSearch);
            if (GUILayout.Button("All", GUILayout.Width(50)))
                foreach (var s in _scriptManager.Scripts) s.IsSelected = true;
            if (GUILayout.Button("None", GUILayout.Width(60)))
                foreach (var s in _scriptManager.Scripts) s.IsSelected = false;
            GUILayout.EndHorizontal();

            var filtered = string.IsNullOrWhiteSpace(_scriptSearch)
                ? _scriptManager.Scripts
                : _scriptManager.Scripts.Where(s => s.FileName.ToLower().Contains(_scriptSearch.ToLower())).ToList();

            _scriptScroll = EditorGUILayout.BeginScrollView(_scriptScroll, GUILayout.Height(200));
            foreach (var s in filtered)
            {
                GUILayout.BeginHorizontal();
                s.IsSelected = EditorGUILayout.ToggleLeft(s.FileName, s.IsSelected, GUILayout.Width(220));
                GUILayout.Label($"[{s.Directory}]", EditorStyles.miniLabel);
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        void DrawNeonSection(Action drawContent)
        {
            GUIStyle cardStyle = new GUIStyle(GUI.skin.box);
            cardStyle.normal.background = MakeTex(2, 2, new Color(0.13f, 0.14f, 0.18f, 0.94f));
            cardStyle.margin = new RectOffset(12, 12, 0, 0);
            cardStyle.padding = new RectOffset(14, 14, 10, 10);
            EditorGUILayout.BeginVertical(cardStyle);
            drawContent.Invoke();
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }

        Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
                pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        void DrawWideNeonButton(string label, Action onClick, Color neon, float height = 36)
        {
            var rect = GUILayoutUtility.GetRect(new GUIContent(label), EditorStyles.miniButton, GUILayout.Height(height), GUILayout.ExpandWidth(true));
            var isHover = rect.Contains(Event.current.mousePosition);

            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = neon;
            if (isHover) EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            GUIStyle neonBtn = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = _glowWhite },
                active = { textColor = _hotPink }
            };
            if (GUI.Button(rect, label, neonBtn))
                onClick?.Invoke();
            GUI.backgroundColor = prevBg;
        }

        void GenerateDiagram(ExportFormat format)
        {
            if (!_scriptsScanned || _scriptManager.Scripts.Count == 0)
            {
                _status = "❌ No scripts scanned. Click 'Scan scripts' first.";
                EditorUtility.DisplayDialog("Error", "No scripts have been scanned.", "OK");
                return;
            }

            var selectedScripts = _scriptManager.GetSelected();
            if (selectedScripts.Count == 0)
            {
                _status = "❗ No script selected!";
                EditorUtility.DisplayDialog("Error", "Select at least one script to include in the diagram.", "OK");
                return;
            }

            var parser = new CSharpParser();
            var umlClasses = new List<UmlClass>();

            foreach (var script in selectedScripts)
            {
                string content = File.ReadAllText(script.Path);
                umlClasses.AddRange(parser.ParseClasses(content));
            }

            if (umlClasses.Count == 0)
            {
                _status = "❗ No classes detected. Check your scripts or parser logic.";
                return;
            }

            string plantuml = PlantUmlGenerator.GeneratePlantUml(umlClasses, _includeAssociations);

            if (format == ExportFormat.PlantUMLFile)
            {
                File.WriteAllText(_outputPath, plantuml, Encoding.UTF8);
                AssetDatabase.Refresh();
                _status = $"✅ Diagram generated: {_outputPath}\n\nClasses: {umlClasses.Count}";
                EditorUtility.DisplayDialog("Done!", $"Diagram generated: {_outputPath}\nOpen it with PlantUML!", "OK");
            }
            else if (format == ExportFormat.PlantUML_URL)
            {
                _lastDiagramURL = PlantUMLTextToUrl(plantuml);
                _status = "✅ Diagram URL generated! Copy-paste it in your browser.";
                EditorUtility.DisplayDialog("Done!", "URL generated below!\nCopy-paste it in your browser.", "OK");
            }
        }

        public static string PlantUMLTextToUrl(string uml)
        {
            byte[] data = Encoding.UTF8.GetBytes(uml);
            using (var ms = new MemoryStream())
            {
                using (var ds = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionLevel.Optimal, true))
                {
                    ds.Write(data, 0, data.Length);
                }
                var deflated = ms.ToArray();
                string encoded = PlantUmlBase64Encode(deflated);
                return "https://www.plantuml.com/plantuml/uml/" + encoded;
            }
        }
        private static readonly string _encode = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_";
        public static string PlantUmlBase64Encode(byte[] data)
        {
            var sb = new StringBuilder();
            int curr = 0, bits = 0;
            foreach (byte b in data)
            {
                curr = (curr << 8) | b;
                bits += 8;
                while (bits >= 6)
                {
                    bits -= 6;
                    sb.Append(_encode[(curr >> bits) & 0x3F]);
                }
            }
            if (bits > 0)
                sb.Append(_encode[(curr << (6 - bits)) & 0x3F]);
            return sb.ToString();
        }

        // --- Classes internes / parser / PlantUML generator (inchangé) ---

        public class UmlClass
        {
            public string Name;
            public string BaseClass;
            public List<string> Interfaces = new();
            public bool IsAbstract;
            public bool IsInterface;
            public List<UmlField> Fields = new();
            public List<UmlProperty> Properties = new();
            public List<UmlMethod> Methods = new();
            public string Summary;
        }
        public class UmlField
        {
            public string Name;
            public string Type;
            public string Visibility;
        }
        public class UmlProperty
        {
            public string Name;
            public string Type;
            public string Visibility;
        }
        public class UmlMethod
        {
            public string Name;
            public string ReturnType;
            public string Visibility;
            public List<UmlParameter> Parameters = new();
        }
        public class UmlParameter
        {
            public string Name;
            public string Type;
        }

        public class CSharpParser
        {
            private static Regex ClassRegex = new(@"(?:///(.*)\n)?\s*(public|private|internal|protected|abstract|sealed|static|partial|\s)*\s*(class|interface)\s+(\w+)(\s*:\s*([\w,\s<>]+))?", RegexOptions.Multiline);
            private static Regex FieldRegex = new(@"(public|private|protected|internal)\s+([\w<>,\[\]]+)\s+(\w+)\s*;", RegexOptions.Multiline);
            private static Regex PropertyRegex = new(@"(public|private|protected|internal)\s+([\w<>,\[\]]+)\s+(\w+)\s*\{\s*get;\s*set;\s*\}", RegexOptions.Multiline);
            private static Regex MethodRegex = new(@"(public|private|protected|internal)\s+([\w<>,\[\]]+)\s+(\w+)\s*\(([^)]*)\)", RegexOptions.Multiline);
            private static Regex InterfaceRegex = new(@"interface\s+(\w+)", RegexOptions.Multiline);

            public List<UmlClass> ParseClasses(string content)
            {
                var classes = new List<UmlClass>();

                foreach (Match match in ClassRegex.Matches(content))
                {
                    var summary = match.Groups[1].Success ? match.Groups[1].Value.Trim() : string.Empty;
                    var isInterface = match.Groups[3].Value == "interface";
                    var className = match.Groups[4].Value;
                    var bases = match.Groups[6].Success ? match.Groups[6].Value.Split(',').Select(s => s.Trim()).ToArray() : null;
                    string baseClass = null;
                    List<string> interfaces = new();
                    if (bases != null && bases.Length > 0)
                    {
                        if (!isInterface)
                        {
                            baseClass = bases[0];
                            if (bases.Length > 1) interfaces.AddRange(bases.Skip(1));
                        }
                        else
                        {
                            interfaces.AddRange(bases);
                        }
                    }

                    var c = new UmlClass
                    {
                        Name = className,
                        BaseClass = baseClass,
                        Interfaces = interfaces,
                        IsAbstract = match.Value.Contains("abstract"),
                        IsInterface = isInterface,
                        Summary = summary
                    };

                    foreach (Match field in FieldRegex.Matches(content))
                    {
                        c.Fields.Add(new UmlField
                        {
                            Visibility = GetVisibilitySymbol(field.Groups[1].Value),
                            Type = field.Groups[2].Value,
                            Name = field.Groups[3].Value
                        });
                    }
                    foreach (Match prop in PropertyRegex.Matches(content))
                    {
                        c.Properties.Add(new UmlProperty
                        {
                            Visibility = GetVisibilitySymbol(prop.Groups[1].Value),
                            Type = prop.Groups[2].Value,
                            Name = prop.Groups[3].Value
                        });
                    }
                    foreach (Match method in MethodRegex.Matches(content))
                    {
                        if (c.Name == method.Groups[3].Value) continue;
                        c.Methods.Add(new UmlMethod
                        {
                            Visibility = GetVisibilitySymbol(method.Groups[1].Value),
                            ReturnType = method.Groups[2].Value,
                            Name = method.Groups[3].Value,
                            Parameters = ParseParameters(method.Groups[4].Value)
                        });
                    }

                    classes.Add(c);
                }

                return classes;
            }

            private static string GetVisibilitySymbol(string keyword)
            {
                if (keyword.Contains("public")) return "+";
                if (keyword.Contains("private")) return "-";
                if (keyword.Contains("protected")) return "#";
                if (keyword.Contains("internal")) return "~";
                return "";
            }

            private static List<UmlParameter> ParseParameters(string raw)
            {
                var parameters = new List<UmlParameter>();
                if (string.IsNullOrWhiteSpace(raw)) return parameters;
                foreach (var param in raw.Split(','))
                {
                    var parts = param.Trim().Split(' ');
                    if (parts.Length == 2)
                        parameters.Add(new UmlParameter { Type = parts[0], Name = parts[1] });
                }
                return parameters;
            }
        }

        public static class PlantUmlGenerator
        {
            public static string GeneratePlantUml(List<UmlClass> classes, bool includeAssociations)
            {
                var sb = new StringBuilder();
                sb.AppendLine("@startuml");
                foreach (var c in classes)
                {
                    string stereotype = c.IsInterface ? " <<interface>>" : (c.IsAbstract ? " <<abstract>>" : "");
                    sb.AppendLine($"class {c.Name}{stereotype} {{");

                    foreach (var f in c.Fields)
                        sb.AppendLine($"    {f.Visibility} {f.Name} : {f.Type}");

                    foreach (var p in c.Properties)
                        sb.AppendLine($"    {p.Visibility} {p.Name} : {p.Type} {{ get; set; }}");

                    foreach (var m in c.Methods)
                    {
                        var paramsList = string.Join(", ", m.Parameters.Select(p => $"{p.Name} : {p.Type}"));
                        sb.AppendLine($"    {m.Visibility} {m.Name}({paramsList}) : {m.ReturnType}");
                    }

                    sb.AppendLine("}");
                    if (!string.IsNullOrWhiteSpace(c.Summary))
                        sb.AppendLine($"' {c.Name}: {c.Summary}");
                }

                foreach (var c in classes)
                {
                    if (!string.IsNullOrEmpty(c.BaseClass))
                        sb.AppendLine($"{c.BaseClass} <|-- {c.Name}");
                    foreach (var iface in c.Interfaces)
                        sb.AppendLine($"{iface} <|.. {c.Name}");
                }

                if (includeAssociations)
                {
                    var classNames = new HashSet<string>(classes.Select(cl => cl.Name));
                    var added = new HashSet<string>();

                    foreach (var c in classes)
                    {
                        foreach (var f in c.Fields)
                        {
                            if (classNames.Contains(f.Type) && f.Type != c.Name)
                            {
                                string key = $"{c.Name}-{f.Type}-field";
                                if (added.Add(key))
                                    sb.AppendLine($"{c.Name} --> {f.Type} : field");
                            }
                        }
                        foreach (var p in c.Properties)
                        {
                            if (classNames.Contains(p.Type) && p.Type != c.Name)
                            {
                                string key = $"{c.Name}-{p.Type}-property";
                                if (added.Add(key))
                                    sb.AppendLine($"{c.Name} --> {p.Type} : property");
                            }
                        }
                        foreach (var m in c.Methods)
                        {
                            foreach (var param in m.Parameters)
                            {
                                if (classNames.Contains(param.Type) && param.Type != c.Name)
                                {
                                    string key = $"{c.Name}-{param.Type}-param";
                                    if (added.Add(key))
                                        sb.AppendLine($"{c.Name} --> {param.Type} : parameter");
                                }
                            }
                        }
                    }
                }
                sb.AppendLine("@enduml");
                return sb.ToString();
            }
        }

        // ----------------------------------------------
        // Système de scan/sélection factorisé et simple
        // ----------------------------------------------
        public class ScriptEntry
        {
            public string Path;
            public bool IsSelected = true;
            public string FileName => System.IO.Path.GetFileName(Path);
            public string Directory => System.IO.Path.GetDirectoryName(Path);
        }

        public class ScriptSelectionManager
        {
            public List<ScriptEntry> Scripts { get; private set; } = new List<ScriptEntry>();

            public void Scan(string folder, string extension = ".cs")
            {
                Scripts.Clear();
                if (!Directory.Exists(folder)) return;
                var files = Directory.GetFiles(folder, "*" + extension, SearchOption.AllDirectories);
                Scripts = files.Select(f => new ScriptEntry { Path = f, IsSelected = true }).ToList();
            }

            public List<ScriptEntry> GetSelected() => Scripts.Where(s => s.IsSelected).ToList();
        }
    }
}
