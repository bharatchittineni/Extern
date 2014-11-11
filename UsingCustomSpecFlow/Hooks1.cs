using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.UnitTestProvider;


namespace UsingCustomSpecFlow
{
    Binding]
    public class Hooks1
    {

        private static string prevSmokeScenarioName = "H";
        // For additional details on SpecFlow hooks see http://go.specflow.org/doc-hooks")]
       [BeforeScenario]
        public void BeforeScenario()
        {

           string x= ScenarioContext.Current.ScenarioInfo.Title;
           if(string.Compare(prevSmokeScenarioName,x,true)==0){
               var unitTestRuntimeProvider = (IUnitTestRuntimeProvider)ScenarioContext.Current.GetBindingInstance((typeof(IUnitTestRuntimeProvider)));
               unitTestRuntimeProvider.TestIgnore("ignored"); 
           }
           prevSmokeScenarioName = x;
        }


        [AfterScenario]
        public void AfterScenario()
        {
            //TODO: implement logic that has to run after executing each scenario
        }
    }
}
