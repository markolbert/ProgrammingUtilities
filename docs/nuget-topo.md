# Topological Sort and Actions

This assembly provides:

- an implementation of a topological sort. These are sorts where the order is implicit in how the objects depend on each other. A common example is determining the build order for a multi-project solution when it's compiled.

- a system for organizing instances of classes which themselves each act on a collection of objects. I needed this for a complex database project where multiple objects had to be processed in multiple ways, but the sequence of processes had to be topologically sorted.

This assembly targets Net5 and has nullability enabled.

For more information consult the [github documentation](https://github.com/markolbert/ProgrammingUtilities).
