using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ShortestWalk")]
[assembly: AssemblyDescription("Rhinoceros 5 WIP ShortestWalk command. Contact giulio@mcneel.com")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("McNeel Europe - Creative Common, Attribution")]
[assembly: AssemblyProduct("ShortestWalk")]
[assembly: AssemblyCopyright("© 2010 McNeel Europe. Released under http://creativecommons.org/licenses/by/3.0/es/")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("97441433-1ff3-4de1-9ae5-1afcc8ef155d")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("1.0.1.0")]
[assembly: AssemblyFileVersion("1.0.1.0")]

[assembly:CLSCompliant(true)]

[assembly: Rhino.PlugIns.PlugInDescription(Rhino.PlugIns.DescriptionType.WebSite, @"http://www.food4rhino.com/shortestwalk")]
[assembly: Rhino.PlugIns.PlugInDescription(Rhino.PlugIns.DescriptionType.Address, @"Roger de Flor, 32-34 Barcelona")]
[assembly: Rhino.PlugIns.PlugInDescription(Rhino.PlugIns.DescriptionType.Country, @"Spain")]
[assembly: Rhino.PlugIns.PlugInDescription(Rhino.PlugIns.DescriptionType.Email, @"giulio@mcneel.com")]
[assembly: Rhino.PlugIns.PlugInDescription(Rhino.PlugIns.DescriptionType.Organization, @"McNeel Europe")]
[assembly: Rhino.PlugIns.PlugInDescription(Rhino.PlugIns.DescriptionType.Phone, @"+34 933 199 002")]
[assembly: Rhino.PlugIns.PlugInDescription(Rhino.PlugIns.DescriptionType.Fax, @"+34 933 195 833")]