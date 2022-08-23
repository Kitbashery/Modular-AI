All code contributions are subject to the following requirements:

All C# code must be up to Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).
In the past Unity used conventions like using `m_variableName` and such practices can still be found in legacy code. However going forward Microsoft's conventions now the standard.

In addition to the C# coding standards there is a prefered comment and region structure to follow.

* Encapsulate all properties in a #region labeled: `Properties:`
* Encapsulate all Monobehaviour Initializers, Updates, Gizmos and IEnumerators in a #region labeled: `Initialization & Updates`.
* Emcapsulate Methods in a #region labeled: `Methods:` within this region you can have sub regions labeled to your liking.

Triple slash summary comments must be used on all public methods and properties that are not obviously named. 
These summary comments corralate to the descriptions in the documentation.
```csharp
///<summary>
/// your summary...
///</summary>
YourMethod()
{
  //your code.
}
```

Sometimes when there is a private field/method that isn't very well named it is a good idea to add a summary comment there as well.

Additionally over any public property field that will be displayed in Unity's inspector window include:
```csharp
[ToolTip("Your tooltip text")]
``` 

Sometimes with very obviouslty named fields that are self explanitory it isn't necessary to add a tooltip attribute, but in most cases it is prefered.


After putting in a pull request it is helpful (but not necessary) to also document them by putting in a pull request to [The documentation repo](https://github.com/Kitbashery/kitbashery.github.io/tree/main/docs) and update the docs.
Doing so however does not mean your additions/changes will be accepted so it may be best to add docs after your changes have been merged or let an official developer document the changes.


Code that doesn't follow these guidelines may not be merged untill it is revised and/or fits within the scope of the project.
