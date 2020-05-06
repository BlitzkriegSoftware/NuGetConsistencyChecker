# NuGet Consistency Checker 

**Q1**. What's it for? 

*A1*. To run against a tree of .NET projects soaking up all of the NuGet packages references, versions, and what projects are using them. Then it sorts them and can output the data as:

* HTML (nice human readable)
* JSON (for automation)
* CSV (for analysis in Excel for example)
* TXT (because, why not)

**Q2**. What problem does it solve?

*A2*. It helps teams with lots of projects keep their NuGet library versions up to date and consistant, especially if they are not rolled into the same solution (where the VS NuGet package manager shows useful stuff).

**Q3**. What else is it good for?

*A3*. Complaince Reporting for orgs that need to report on the technologies they are using

**Q4**. Whats a good way to use it?

*A4*. Run it against your source tree on some cadence. Some users report they run it in their CI/CD build engine daily or weekly and then compare to the previous version (using the JSON format) to spot potentially bad things. 


## Two Versions:
* .NET Framework in the `dotnetframework/`
* .NET Core `dotnetcore/`
 
# About 

Stuart Williams

* Cloud/DevOps Practice Lead
 
* Magenic Technologies Inc.
* Office of the CTO
 
* <a href="mailto:stuartw@magenic.com" target="_blank">stuartw@magenic.com</a> (e-mail)

* Blog: <a href="http://blitzkriegsoftware.net/Blog" target="_blank">http://blitzkriegsoftware.net/Blog</a> 
* LinkedIn: <a href="http://lnkd.in/P35kVT" target="_blank">http://lnkd.in/P35kVT</a> 

* YouTube: <a href="https://www.youtube.com/channel/UCO88zFRJMTrAZZbYzhvAlMg" target="_blank">https://www.youtube.com/channel/UCO88zFRJMTrAZZbYzhvAlMg</a> 

