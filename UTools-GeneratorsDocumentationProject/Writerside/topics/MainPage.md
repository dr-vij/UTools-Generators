# UTools-SourceGenerators
## Subscriptions

This plugin is a Roslyn Source Generators tool that can be used for the automation of some subscription boilerplate routines. 
Add the DisposableSubscription or EventSubscription attribute to a field to automatically generate the subscription code and property for it.

It will create static subscriptions/properties/events in case you use static.

## Installation

- Add UToolsGenerators.dll and UToolsAttributes.dll to your UNITY project.
- Create an assembly for them.
- Mark UToolsGenerators.dll as a Roslyn Analyzer (add the RoslynAnalyzer tag on it; see [UNITY documentation](https://docs.unity3d.com/2023.3/Documentation/Manual/roslyn-analyzers.html) for more details).
- Make a link to the assembly in your project. The linked assembly will receive Source Generator benefits.

<note>
    <p>
        Your class must be partial to use this feature. Global namespaces are not supported yet.
    </p>
</note>
