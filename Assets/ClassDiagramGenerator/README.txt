# ClassDiagramGenerator

A modern Unity Editor tool to **generate UML class diagrams from all C# scripts** in your `Assets/Scripts` folder.  
Export PlantUML-compatible diagrams as files or shareable URLs. Fast, simple, and integrated into the Unity Editor.

---

## How to Install

This tool is provided as a **C# script** for the Unity Editor.

1. Copy the `ClassDiagramGenerator.cs` script into your project's `Assets/Editor` folder.
2. Make sure you have your C# scripts to document in `Assets/Scripts`.

---

## Usage

1. **Open the Tool**

   - In Unity, open the menu: `Tools > 🧬 Generate Class Diagram`.

2. **Select Export Format**

   - `📄 PlantUML File` : generates a `.puml` file in `Assets/Scripts/ClassDiagram.puml`.
   - `🌐 PlantUML URL` : generates a direct link to view your diagram online.

3. **Generate the Diagram**

   - Click **🛠️ Generate Diagram**.

4. **Visualize**

   - If you selected URL, copy the generated link and open it in your browser to visualize the diagram.

---

## Features

- User-friendly graphical interface in Unity (menu Tools).
- Automatically scans all C# scripts in `Assets/Scripts`.
- Export as PlantUML file or PlantUML Online URL.
- Displays and supports easy copy of the generated URL.
- Detects classes, fields, properties, methods, and inheritance relationships.

---

## Notes

- Only analyzes scripts inside `Assets/Scripts`.
- Only supports standard C# class syntax.
- Diagram extraction is based on regular expressions.
- No project files are modified or deleted.

---

## FAQ

**Q: Does it delete files?**  
A: No. The tool only generates a `.puml` file or URL, nothing is deleted.

**Q: Can I use it outside `Assets/Scripts`?**  
A: Only scripts inside `Assets/Scripts` are analyzed.

**Q: Will it overwrite existing diagrams?**  
A: Yes, generating a new `.puml` file will overwrite any previous one.

---

## Support

For questions, suggestions, or bug reports, please contact:  
jules.gilli@live.fr

---

## License

MIT License

---

ClassDiagramGenerator © 2025
