using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revit.Async;
using Revit.Async.Interfaces;
using Autodesk.Revit.UI;
using RTF.Applications;
using NUnit.Framework;
using RTF.Framework;
using Autodesk.Revit.DB;

namespace RevitAsync.tests
{

    public class TestTasBehavior : IRevitTaskBehavior
    {
        public Task<TResult> RaiseGlobal<THandler, TParameter, TResult>(TParameter parameter) where THandler : IGenericExternalEventHandler<TParameter, TResult>
        {
            throw new NotImplementedException();
        }

        public void RegisterGlobal<TParameter, TResult>(IGenericExternalEventHandler<TParameter, TResult> handler)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> RunAsync<TResult>(Func<UIApplication, TResult> function)
        {
            return Task.FromResult( function(RevitTestExecutive.CommandData.Application));
        }

        public Task<TResult> RunAsync<TResult>(Func<UIApplication, Task<TResult>> function)
        {
            return Task.FromResult(function(RevitTestExecutive.CommandData.Application).GetAwaiter().GetResult());
        }
    }

    [TestFixture]
    public class RevitAsyncRTFIntegrationsTests
    {
        public RevitAsyncRTFIntegrationsTests()
        {
            RevitTask.Initialize(new TestTasBehavior());
        }

        [Test]
        [TestModel("prj.rvt")]
        public void TestShouldPass()
        {
            var name = RevitTask.RunAsync(app =>
            {
                var wall = new FilteredElementCollector(app.ActiveUIDocument.Document)
                    .OfClass(typeof(Wall))
                    .Cast<Wall>()
                    .First();
                return wall.Name;
            }).GetAwaiter().GetResult();
            Assert.That(name, Is.EqualTo("Типовой - 200мм"));
        }

        [Test]
        [TestModel("prj.rvt")]
        public void TestShouldPassWithTask()
        {
            var name = RevitTask.RunAsync(async app =>
            {
                return await RevitTask.RunAsync(_ =>
                {
                    var wall = new FilteredElementCollector(app.ActiveUIDocument.Document)
                        .OfClass(typeof(Wall))
                        .Cast<Wall>()
                        .First();
                    return wall.Name;
                });
            }).GetAwaiter().GetResult();
            Assert.That(name, Is.EqualTo("Типовой - 200мм"));
        }
    }
}
