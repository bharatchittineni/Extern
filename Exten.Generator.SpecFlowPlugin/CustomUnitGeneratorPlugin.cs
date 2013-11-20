using TechTalk.SpecFlow.Infrastructure;

[assembly: GeneratorPlugin(typeof(Exten.Generator.SpecFlowPlugin.CustomUnitGeneratorPlugin))]


namespace Exten.Generator.SpecFlowPlugin
{
    using TechTalk.SpecFlow.Utils;
    using TechTalk.SpecFlow.UnitTestProvider;
    using TechTalk.SpecFlow.Generator.Configuration;    
    using TechTalk.SpecFlow.Generator.Plugins;
    using TechTalk.SpecFlow.Generator.UnitTestProvider;
    using TechTalk.SpecFlow.Generator;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using TechTalk.SpecFlow;
    using TechTalk.SpecFlow.Bindings;
    using System.IO;
    using System;
    using System.Xml.Linq;
    using TechTalk.SpecFlow.Generator.Interfaces;
    using TechTalk.SpecFlow.Configuration;
    using TechTalk.SpecFlow.Tracing;
    

    class CustomUnitGeneratorPlugin: IGeneratorPlugin
    {
        public void RegisterConfigurationDefaults(SpecFlowProjectConfiguration specFlowConfiguration)
        {
        }

        public void RegisterCustomizations(BoDi.ObjectContainer container, SpecFlowProjectConfiguration generatorConfiguration)
        {
            container.RegisterTypeAs<CustomUnitTestGeneratorProvider, IUnitTestGeneratorProvider>();
            container.RegisterTypeAs<CustomTestRunner, ITestRunner>();
            //container.RegisterTypeAs<CustomGeneratorConfigurationProvider, IGeneratorConfigurationProvider>();
        }

        public void RegisterDependencies(BoDi.ObjectContainer container)
        {
        }
    }
   

    class CustomUnitTestGeneratorProvider : IUnitTestGeneratorProvider
    {
        
        private const string TESTFIXTURE_ATTR = "NUnit.Framework.TestFixtureAttribute";
        private const string TEST_ATTR = "NUnit.Framework.TestAttribute";
        private const string ROW_ATTR = "NUnit.Framework.TestCaseAttribute";
        private const string CATEGORY_ATTR = "NUnit.Framework.CategoryAttribute";
        private const string TESTSETUP_ATTR = "NUnit.Framework.SetUpAttribute";
        private const string TESTFIXTURESETUP_ATTR = "NUnit.Framework.TestFixtureSetUpAttribute";
        private const string TESTFIXTURETEARDOWN_ATTR = "NUnit.Framework.TestFixtureTearDownAttribute";
        private const string TESTTEARDOWN_ATTR = "NUnit.Framework.TearDownAttribute";
        private const string IGNORE_ATTR = "NUnit.Framework.IgnoreAttribute";
        private const string DESCRIPTION_ATTR = "NUnit.Framework.DescriptionAttribute";
        private const string REPEAT_ATTR = "NUnit.Framework.Repeat";
        public CustomUtils utils = new CustomUtils();
        string testsFilePath;

        protected CodeDomHelper CodeDomHelper { get; set; }

        public bool SupportsRowTests { get { return true; } }
        public bool SupportsAsyncTests { get { return false; } }

        public CustomUnitTestGeneratorProvider(CodeDomHelper codeDomHelper)
        {
            CodeDomHelper = codeDomHelper;
        }

        public void SetTestClass(TestClassGenerationContext generationContext, string featureTitle, string featureDescription)
        {
            CodeDomHelper.AddAttribute(generationContext.TestClass, TESTFIXTURE_ATTR);
            CodeDomHelper.AddAttribute(generationContext.TestClass, DESCRIPTION_ATTR, featureTitle);
            testsFilePath = Path.Combine(utils.QueryData(), featureTitle.ToIdentifier()+".txt");
        }

        public void SetTestClassCategories(TestClassGenerationContext generationContext, IEnumerable<string> featureCategories)
        {
            CodeDomHelper.AddAttributeForEachValue(generationContext.TestClass, CATEGORY_ATTR, featureCategories);
        }

        public void SetTestClassIgnore(TestClassGenerationContext generationContext)
        {
            CodeDomHelper.AddAttribute(generationContext.TestClass, IGNORE_ATTR);
        }

        public virtual void FinalizeTestClass(TestClassGenerationContext generationContext)
        {
            // by default, doing nothing to the final generated code
        }


        public void SetTestClassInitializeMethod(TestClassGenerationContext generationContext)
        {
            CodeDomHelper.AddAttribute(generationContext.TestClassInitializeMethod, TESTFIXTURESETUP_ATTR);
        }

        public void SetTestClassCleanupMethod(TestClassGenerationContext generationContext)
        {
            CodeDomHelper.AddAttribute(generationContext.TestClassCleanupMethod, TESTFIXTURETEARDOWN_ATTR);
        }


        public void SetTestInitializeMethod(TestClassGenerationContext generationContext)
        {
            CodeDomHelper.AddAttribute(generationContext.TestInitializeMethod, TESTSETUP_ATTR);
        }

        public void SetTestCleanupMethod(TestClassGenerationContext generationContext)
        {
            CodeDomHelper.AddAttribute(generationContext.TestCleanupMethod, TESTTEARDOWN_ATTR);
        }


        public void SetTestMethod(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle)
        {

            int repeatCount = 0;
            CodeDomHelper.AddAttribute(testMethod, TEST_ATTR);
            CodeDomHelper.AddAttribute(testMethod, DESCRIPTION_ATTR, scenarioTitle);
            CodeDomHelper.AddAttribute(testMethod, REPEAT_ATTR, repeatCount);
            //testsFilePath=Path.Combine(utils.QueryData(), scenarioTitle.ToIdentifier());
            utils.WritingTestNamesToFile(testsFilePath, scenarioTitle.ToIdentifier());
        }

        public void SetTestMethodCategories(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<string> scenarioCategories)
        {
            CodeDomHelper.AddAttributeForEachValue(testMethod, CATEGORY_ATTR, scenarioCategories);
        }

        public void SetTestMethodIgnore(TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
        {
            CodeDomHelper.AddAttribute(testMethod, IGNORE_ATTR);
        }


        public void SetRowTest(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle)
        {
            SetTestMethod(generationContext, testMethod, scenarioTitle);
        }

        public void SetRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<string> arguments, IEnumerable<string> tags, bool isIgnored)
        {
            var args = arguments.Select(
              arg => new CodeAttributeArgument(new CodePrimitiveExpression(arg))).ToList();

            // addressing ReSharper bug: TestCase attribute with empty string[] param causes inconclusive result - https://github.com/techtalk/SpecFlow/issues/116
            var exampleTagExpressionList = tags.Select(t => new CodePrimitiveExpression(t)).ToArray();
            CodeExpression exampleTagsExpression = exampleTagExpressionList.Length == 0 ?
                (CodeExpression)new CodePrimitiveExpression(null) :
                new CodeArrayCreateExpression(typeof(string[]), exampleTagExpressionList);
            args.Add(new CodeAttributeArgument(exampleTagsExpression));

            if (isIgnored)
                args.Add(new CodeAttributeArgument("Ignored", new CodePrimitiveExpression(true)));

            CodeDomHelper.AddAttribute(testMethod, ROW_ATTR, args.ToArray());
        }

        public void SetTestMethodAsRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle, string exampleSetName, string variantName, IEnumerable<KeyValuePair<string, string>> arguments)
        {
            // doing nothing since we support RowTest
        }
    }

    public class CustomTestRunner : ITestRunner
    {
        private readonly ITestExecutionEngine executionEngine;

        public CustomTestRunner(ITestExecutionEngine executionEngine)
        {
            this.executionEngine = executionEngine;
        }

        public FeatureContext FeatureContext
        {
            get { return executionEngine.FeatureContext; }
        }

        public ScenarioContext ScenarioContext
        {
            get { return executionEngine.ScenarioContext; }
        }

        public void InitializeTestRunner(Assembly[] bindingAssemblies)
        {
            executionEngine.Initialize(bindingAssemblies);
        }

        public void OnFeatureStart(FeatureInfo featureInfo)
        {
            executionEngine.OnFeatureStart(featureInfo);
        }

        public void OnFeatureEnd()
        {
            executionEngine.OnFeatureEnd();
        }

        public void OnScenarioStart(ScenarioInfo scenarioInfo)
        {
            executionEngine.OnScenarioStart(scenarioInfo);

        }

        public void CollectScenarioErrors()
        {
            executionEngine.OnAfterLastStep();
        }

        public void OnScenarioEnd()
        {
            executionEngine.OnScenarioEnd();
        }

        public void OnTestRunEnd()
        {
            executionEngine.OnTestRunEnd();
        }

        public void Given(string text, string multilineTextArg, Table tableArg, string keyword = null)
        {

            executionEngine.Step(StepDefinitionKeyword.Given, keyword, text, multilineTextArg, tableArg);
        }

        public void When(string text, string multilineTextArg, Table tableArg, string keyword = null)
        {
            executionEngine.Step(StepDefinitionKeyword.When, keyword, text, multilineTextArg, tableArg);
        }

        public void Then(string text, string multilineTextArg, Table tableArg, string keyword = null)
        {
            ScenarioContext.Current["LatestThenStep"] = text;
            executionEngine.Step(StepDefinitionKeyword.Then, keyword, text, multilineTextArg, tableArg);
        }

        public void And(string text, string multilineTextArg, Table tableArg, string keyword = null)
        {
            executionEngine.Step(StepDefinitionKeyword.And, keyword, text, multilineTextArg, tableArg);
        }

        public void But(string text, string multilineTextArg, Table tableArg, string keyword = null)
        {
            executionEngine.Step(StepDefinitionKeyword.But, keyword, text, multilineTextArg, tableArg);
        }

        public void Pending()
        {
            executionEngine.Pending();
        }
    }

    public class CustomUtils
    {
        public static string appConfigPath = Path.Combine(Directory.GetParent(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath).Parent.FullName, "App.config");
        public bool fileCreatedNow = false;
        public string QueryData()
        {
            //static string appConfigPath=Path.Combine (Directory.GetParent(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly(​).Location)).Parent.FullName,"App.config");
            //XDocument xmlConfig=XDocument.Load(appConfigPath);

            var text = new List<string>(from addNodes in XDocument.Load(appConfigPath).Descendants("userConfig").Elements()
                                        where addNodes.Attribute("key").Value == "TestsDirPath"
                                        select addNodes.Attribute("value").Value);

            if (text.Count > 0)
                return text[0];
            else
            {
                throw new WritingTestsToFileException("There is a problem in writing tests: either wrong filepath or wrong setting in the xml");
            }


        }

        public void WritingTestNamesToFile(string fName, string textToWrite)
        {
            if (!(File.Exists(fName) && fileCreatedNow == true))
            {
                if (!Directory.GetParent(fName).Exists)
                    Directory.GetParent(fName).Create();

                File.CreateText(fName).Close();
                fileCreatedNow = true;
            }

            File.AppendAllText(fName, textToWrite + Environment.NewLine);
        }


        private class WritingTestsToFileException : System.Exception
        {


            public WritingTestsToFileException()
            {
            }

            public WritingTestsToFileException(string message)
                : base(message)
            {
            }
        }
    }






}
