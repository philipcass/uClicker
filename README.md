# uClicker

uClicker is a clicker/idle game library that attempts to take the boilerplate out of clicker games made in [Unity](http://unity3d.com). Influenced heavily by the awesome [Idle game maker](http://orteil.dashnet.org/igm/).
  - Plug n' Play
  - (Probably) minimal code required
  - No allocations/garbage

uClicker is very ScriptableObject oriented. Every upgrade, building, currency is a scriptable object. This allows you to create new content without writing a line of code.
A normal workflow would be
- Create a currency (what we're incrementing)
- Create a Clickable (how we're incrementing)
- Create a manager (actually does the incrementing/logic)
- Hook them together.
- `MyClickerManagerReference.Click()` to make the numbers go up
- Create upgrades and buildings to modify those values (And don't forget to periodically `MyClickerManagerReference.Tick()`

### Installation

uClicker now comes in a package. You can add the github URL via the Package manager or download the source and put it into your projects `Packages` folder.

### Example

In Unity 2019.3 and up you should be able to install the same via the package manager, otherwise copy and paste the contents of the `uClicker/Sameples~/` to your Assets folder. 
