# Unity Evacuation Simulation

> 🌐 **Try it in your browser:** [https://joshux.itch.io/unity-evacuation-beta](https://joshux.itch.io/unity-evacuation-beta)

---

## Prerequisites

Before running the project, ensure you have the following installed:

- **Unity Hub** → [https://unity.com/download](https://unity.com/download)
- **Unity Version:** 6.3 LTS

---

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/LiandarJoshua/UnitySimulation.git
```

### 2. Open the Project in Unity

1. Open **Unity Hub**
2. Click **Add**
3. Select the cloned project folder
4. Ensure the correct Unity version is selected
5. Click **Open**
### 3. Open the Main Scene (IMPORTANT): Otherwise it wont spawn

After opening the project: 

1. Go to the **Project** window  (Bottom Left)
2. Navigate to:  Assets → Scenes
3. Double-click **SampleScene** to open it  

> ⚠️ If you skip this step, the scene may appear empty or missing assets.
### 3. Install Required Packages

Unity should automatically install dependencies, but if not:

1. Go to **Window → Package Manager**
2. Verify the following are installed:

**Packages - Asset Store**
| Package | Version |
|---|---|
| City-Themed Low-Poly Characters – Free Pack | 1.0 |
| Fire 001 | 1.0 |

**Packages - Bezi**
| Package | Version |
|---|---|
| Bezi Plugin | 0.79.17 |

**Packages - Unity**
| Package | Version |
|---|---|
| AI Navigation | 2.0.10 |
| Burst | 1.8.28 |
| Collections | 2.6.2 |
| Custom NUnit | 2.0.5 |
| Editor Coroutines | 1.0.1 |
| Input System | 1.18.0 |
| JetBrains Rider Editor | 3.0.39 |
| Mathematics | 1.3.3 |
| Mono Cecil | 1.11.6 |
| Multiplayer Center | 1.0.1 |
| Newtonsoft Json | 3.2.2 |
| Performance Testing API | 3.2.0 |
| Scriptable Render Pipeline Core | 17.3.x |
| Searcher | 4.9.4 |
| Settings Manager | 2.1.1 |
| Shader Graph | 17.3.0 |
| Test Framework | 1.6.0 |
| Timeline | 1.8.10 |
| uGUI | 2.0.0 |
| Unity Version Control | 2.11.3 |
| Universal Render Pipeline | 17.3.0 |
| Universal Render Pipeline Config | 17.0.x |
| Visual Scripting | 1.9.9 |
| Visual Studio Editor | 2.0.26 |
| WebGL Publisher | 4.2.4 |

---

## NavMesh Setup (If Agents Don't Move)
Make sure SampleScene is the scene in use (there is only one scene).
1. Go to **Window → AI → Navigation**
2. Click **Bake**

---

## Running the Simulation
Make sure SampleScene is the scene in use (there is only one scene).

1. Click the **Play** button in Unity
2. Once the scene starts, click the **"Simulator"** button on screen — this will open the simulation UI
3. Use the UI controls to interact with and run the evacuation scenario
