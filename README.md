<div align="center">

# 🐺 Wolf Fun

**Smart AI Farm Management**

[![Unity 6](https://img.shields.io/badge/Unity-6000.0.23f1-black?style=flat&logo=unity)](https://unity.com/)
[![C#](https://img.shields.io/badge/C%23-Latest-purple?style=flat&logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![MIT](https://img.shields.io/badge/License-MIT-green?style=flat)](LICENSE)

![Game Preview](path/to/game-preview.gif)

*Build your automated farm empire with intelligent AI workers*

</div>

---

## ✨ What Makes It Special

<table>
<tr>
<td width="50%">

### 🤖 **Zero-Effort Automation**
Your workers think for themselves. They find tasks, navigate obstacles, and complete work without micromanagement.

</td>
<td width="50%">

### 📈 **Strategic Growth** 
Focus on big decisions while AI handles the details. Buy land, hire workers, optimize profits.

</td>
</tr>
</table>

---

## 🎮 Core Features

<details>
<summary><strong>🌱 Smart Farming System</strong></summary>

- **3 Crop Types**: Tomatoes, Blueberries, Strawberries
- **Auto-Harvest**: Workers know when crops are ready
- **Growth Visualization**: Watch your farm come alive

![Farming System](path/to/farming-demo.gif)

<img width="1710" alt="Screenshot 2025-06-24 at 17 11 41" src="https://github.com/user-attachments/assets/6dc23be4-e21f-412d-ab4b-09ab66e899b2" />


</details>

<details>
<summary><strong>🧠 AI Worker Intelligence</strong></summary>

```
🔄 IDLE → 🚶 MOVING → ⚒️ WORKING → 🔄 IDLE
```

Workers autonomously:
- Find available tasks
- Navigate using Unity NavMesh
- Complete work efficiently
- Return to base when done

![AI Behavior](path/to/ai-demo.gif)

<img width="1710" alt="Screenshot 2025-06-24 at 17 26 40" src="https://github.com/user-attachments/assets/fb90db67-1b17-4204-8a56-64379716d71e" />


</details>

<details>
<summary><strong>🐄 Passive Income Streams</strong></summary>

- **Dairy Cows**: Milk production every few minutes
- **Set & Forget**: No manual management required
- **Scaling Profits**: More cows = more money

![Livestock](path/to/livestock-demo.gif)

<img width="958" alt="Screenshot 2025-06-24 at 17 14 49" src="https://github.com/user-attachments/assets/29bf1348-74c1-469b-bfa0-92fe3267ecbd" />


</details>

<details>
<summary><strong>💰 Economic Strategy</strong></summary>

| Action | Cost | Benefit |
|--------|------|---------|
| Seeds | 50-100g | Crop production |
| Workers | 500g | Task automation |
| Equipment | 1000g+ | Efficiency boost |

**Win Condition**: Reach 1,000,000 gold! 🏆

![Economy](path/to/economy-demo.png)

<img width="1709" alt="Screenshot 2025-06-24 at 17 27 06" src="https://github.com/user-attachments/assets/26fb8a6d-675d-4fe3-9d4c-d24e2ef90df2" />

</details>

---

## ⚡ Quick Start

<div align="center">

### Requirements
![Unity](https://img.shields.io/badge/Unity_6-000000?style=for-the-badge&logo=unity) ![Windows](https://img.shields.io/badge/Windows-0078D6?style=for-the-badge&logo=windows) ![Mac](https://img.shields.io/badge/macOS-000000?style=for-the-badge&logo=apple)

</div>

```bash
# Clone & Play in 3 steps
git clone https://github.com/tsan2711/wolf-fun.git
cd wolf-fun
# Open in Unity 6 → MainMenu scene → Press Play
```

<div align="center">

🎯 **New Game** → 🌱 **Plant Seeds** → 👷 **Hire Workers** → 💰 **Profit!**

![Quick Start](path/to/quickstart-demo.gif)

</div>

---

## 🔧 Under the Hood

<div align="center">

```mermaid
graph LR
    A[🤖 Worker AI] --> B[🎯 Task Detection]
    B --> C[🗺️ NavMesh Pathfinding]
    C --> D[⚒️ Task Execution]
    D --> A
```

</div>

### State Machine Magic

```csharp
// Clean, simple AI behavior
public class WorkerStateMachine {
    Idle → Moving → Working → Idle
    
    // Workers decide what to do next
    HasTask() && !AtTarget() → Moving
    AtTarget() → Working
    TaskComplete() → Idle
}
```

### Performance Features
- ⚡ **Object Pooling** - Smooth 60+ FPS
- 🎯 **Event-Driven UI** - Updates only when needed  
- 🧭 **Optimized Pathfinding** - Smart navigation

---

## 🎮 Gameplay Tips

<div align="center">

| 🥇 **Beginner** | 🥈 **Intermediate** | 🥉 **Expert** |
|----------------|-------------------|---------------|
| Start with tomatoes | Mix crop types | Optimize worker ratios |
| Buy 2-3 workers | Focus on equipment | Strategic plot placement |
| Manual oversight | Trust the AI | Full automation |

</div>

---

## 🛠️ Developer Tools

> **Debug Commands** - Right-click worker in Scene view

```csharp
[ContextMenu("🔄 Reset to Idle")]     // Fix stuck workers
[ContextMenu("📊 Print State")]       // Debug current status  
[ContextMenu("✅ Complete Task")]      // Force task completion
```

---

## 📥 Download

<div align="center">

[![Download for Windows](https://img.shields.io/badge/Download-Windows-blue?style=for-the-badge&logo=windows)](https://github.com/tsan2711/wolf-fun/releases/download/v1.0/tansang-wolf-fun-testing-windows-v1.0.zip)
[![Download for Mac](https://img.shields.io/badge/Download-macOS-black?style=for-the-badge&logo=apple)](https://github.com/tsan2711/wolf-fun/releases/download/v1.0/tansang-wolf-fun-testing-macos-v1.0.zip)

**System Requirements**: Windows 10+ or macOS 10.15+ • 4GB RAM • DirectX 11

</div>

---

## 🎮 Gameplay Videos

<div align="center">

### 🤖 AI Workers in Action

https://github.com/user-attachments/assets/1d5d1c07-8b78-4ca7-9aea-dac4414171c4

### 🌱 Farm Management Showcase  

https://github.com/user-attachments/assets/8e4ded7b-2a68-421c-b111-27c02bfbd445

*Watch the intelligent AI system automate your entire farm operation*

</div>

---

<div align="center">

### Made with 💜 using Unity 6

![Built with Unity](https://img.shields.io/badge/Built_with-Unity_6-black?style=for-the-badge&logo=unity)
![Clean Code](https://img.shields.io/badge/Clean-Architecture-blue?style=for-the-badge)
![State Machine](https://img.shields.io/badge/AI-State_Machine-green?style=for-the-badge)

**[⭐ Star this repo](https://github.com/tsan2711/wolf-fun) if you found it useful!**

</div>
