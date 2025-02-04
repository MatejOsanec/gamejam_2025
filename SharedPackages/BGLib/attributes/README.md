# Attributes
Holds all the attributes used by the Beat Games Library and Beat Saber.

## Getting started

This package is a leaf package that will be used by many assemblies to remove dependencies of other assemblies only to use Attributes.

For example, you have a feature in your game that holds a Localization Key, but you don't want to access the Polyglot/Localization directly.
All you need is the LocalizationKey to differ that string from other strings.
By referencing this package/assembly you will be able to achieve that without the dependency to Polyglot

