# XmlSerializable
Xml Serialization Library for .Net

## Overview

The XmlSerializer type in .Net hasn't been updated or improved since the original release of the .Net Framework. It's flexibility is convenient in that it makes is possible to create a mapping between C# objects and almost any XML representation. However it doesn't support basic features of .Net such as private setters or generic dictionary collections. This library is designed to be a drop in replacement for XmlSerializer that supports these features of .Net. Objects intended for use with XmlSerializable can be used interchangeably with objects designed for XmlSerialzer.
