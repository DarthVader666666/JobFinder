```mermaid
flowchart LR
  subgraph API[JobFinders.Api]
    Controllers(Controllers)
  end

  subgraph Application[JobFinders.Application]
    direction LR
    AzureEmailSender(AzureEmailSender)
    HtmlLoader(HtmlLoader)
    JobFinderManager(JobFinderManager)    
    PageObserver(PageObserver)
    Transliterator(Transliterator)
  end

  subgraph DAL[JobFinders.DAL]
    direction LR
    Repository[Repository]
    UOW[UnitOfWork]
  end

  subgraph Domain[JobFinders.Domain]
    direction LR
    Entities[Entities]
    Interfaces[Interfaces]
    Models[Models]
  end

  Domain --> DAL
  Domain --> Application
  Client[Web Client / VueJS] <--> API

  API <-- Application
  API <-- DAL
  API <-- Domain
  DAL <-.-> DB[(SQL DB)]
```
