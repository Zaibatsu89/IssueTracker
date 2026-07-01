# WT_Workflow_05

| From | Transition | Screen | To |
|---|---|---|---|
| **Implementing** | Implemented | No Screen | → Testing |
| | implemented and tested | No Screen | → Done |
| **Testing** | Ext.review req. | No Screen | → Review External |
| | TestNok | No Screen | → Implementing |
| | Test OK | No Screen | → Done |
| | Niet Reproduceerbaar (T) | No Screen | → Monitoring |
| | Tested | No Screen | → Review Final |
| **Done** | ProjectUpdated | No Screen | → Resolved |
| | CheckFailed | No Screen | → Implementing |
| **Canceled** | *There are no transitions out of this status* | | |
| **Analysing** | Create PoC | No Screen | → PoC / Feasibility |
| | Analyzed | No Screen | → Designing |
| | review request | No Screen | → Review Analyse |
| | Implement | No Screen | → Implementing |
| | Niet Reproduceerbaar (A) | No Screen | → Monitoring |
| | Handle external | No Screen | → External |
| **Review design** | ReviewOK | No Screen | → Implementing |
| | Review Nok | No Screen | → Designing |
| **Blocked** | ContinueImplementation | No Screen | → Implementing |
| | ContinueTest | No Screen | → Testing |
| **Backlog** | Assign | No Screen | → Assigned |
| | Fixed | No Screen | → Done |
| **Monitoring** | *There are no transitions out of this status* | | |
| **Assigned** | Start (Design Available) | No Screen | → Implementing |
| | Re-assign | No Screen | → Backlog |
| | Start | No Screen | → Analysing |
| | Executed | No Screen | → Done |
| | Toekennen | No Screen | → Intake |
| **Review Analyse** | Start Design | No Screen | → Designing |
| **Designing** | review request | No Screen | → Review design |
| | Designed | No Screen | → Implementing |
| | Implement external | No Screen | → External |
| **Request2Cancel** | Duplicate | No Screen | → duplicate |
| | CancelAndClose | No Screen | → Canceled |
| **External** | Delivered from external | No Screen | → Testing |
| **Intake** | Voorstel | No Screen | → Review aanpak |
| **Review aanpak** | Voorstel OK | No Screen | → Analysing |
| **Review Final** | Review OK | No Screen | → Done |
| **SpecialAction** | *There are no transitions out of this status* | | |
| **PoC / Feasibility** | Poc accepted | No Screen | → Designing |
| **duplicate** | *There are no transitions out of this status* | | |
| **Resolved** | backwards comp | No Screen | → Closed |
| **Closed** | *There are no transitions out of this status* | | |
| **Review External** | ext review passed | No Screen | → Done |
