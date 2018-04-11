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

Drop the uClicker folder into your Plugins folder and it should just go.

### Example

uClicker comes packaged with an example scene with basic UI hooked up. You can find this in the `Example` folder.
