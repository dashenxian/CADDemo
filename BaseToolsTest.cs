using System.Collections.Generic;
using System.Linq;
using AcDotNetTool;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AcDotNetToolTest
{
    public class BaseToolsTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void InLineTure()
        {
            #region MyRegion

            string dicStr = "{\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":1917.149692622299,\"Y\":1762.1521804845645},{\"X\":1917.149692622299,\"Y\":1455.7865959353185}]}\":\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":1320.8080073817982,\"Y\":2098.7228279467809},{\"X\":2638.8095603938218,\"Y\":2098.7228279467809},{\"X\":2638.8095603938218,\"Y\":1762.1521804845643},{\"X\":1917.149692622299,\"Y\":1762.1521804845645},{\"X\":1917.149692622299,\"Y\":1455.7865959353185},{\"X\":2707.95062140755,\"Y\":1455.7865959353185},{\"X\":2656.0948180923424,\"Y\":765.38527244931083},{\"X\":1434.65131500248,\"Y\":857.12746920098},{\"X\":1320.8080073817982,\"Y\":2098.7228279467809}]}\",\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":1860.4576283987717,\"Y\":2008.0983294521129},{\"X\":2387.6582526255461,\"Y\":2008.0983294521129},{\"X\":2387.6582526255461,\"Y\":1865.7030550284744},{\"X\":1873.4215678952078,\"Y\":1865.7030550284744},{\"X\":1873.4215678952078,\"Y\":1270.2319144915582},{\"X\":2137.0218875635055,\"Y\":1270.2319144915582},{\"X\":2137.0218875635055,\"Y\":959.55132280191992},{\"X\":1709.2115366552389,\"Y\":1093.3165764796631},{\"X\":1709.2115366552389,\"Y\":1498.9273575835023},{\"X\":1860.4576283987717,\"Y\":2008.0983294521129}]}\":\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":1320.8080073817982,\"Y\":2098.7228279467809},{\"X\":2638.8095603938218,\"Y\":2098.7228279467809},{\"X\":2638.8095603938218,\"Y\":1762.1521804845643},{\"X\":1917.149692622299,\"Y\":1762.1521804845645},{\"X\":1917.149692622299,\"Y\":1455.7865959353185},{\"X\":2707.95062140755,\"Y\":1455.7865959353185},{\"X\":2656.0948180923424,\"Y\":765.38527244931083},{\"X\":1434.65131500248,\"Y\":857.12746920098},{\"X\":1320.8080073817982,\"Y\":2098.7228279467809}]}\",\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":1436.9685956925896,\"Y\":1986.5232872850365},{\"X\":1436.9685956925896,\"Y\":1792.3479142464585},{\"X\":1722.1754912614942,\"Y\":1792.3479142464585},{\"X\":1722.1754912614942,\"Y\":1982.2082801446431},{\"X\":1436.9685956925896,\"Y\":1986.5232872850365}]}\":\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":1320.8080073817982,\"Y\":2098.7228279467809},{\"X\":2638.8095603938218,\"Y\":2098.7228279467809},{\"X\":2638.8095603938218,\"Y\":1762.1521804845643},{\"X\":1917.149692622299,\"Y\":1762.1521804845645},{\"X\":1917.149692622299,\"Y\":1455.7865959353185},{\"X\":2707.95062140755,\"Y\":1455.7865959353185},{\"X\":2656.0948180923424,\"Y\":765.38527244931083},{\"X\":1434.65131500248,\"Y\":857.12746920098},{\"X\":1320.8080073817982,\"Y\":2098.7228279467809}]}\",\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":1525.6459755660762,\"Y\":1646.0832144020414},{\"X\":1454.6646440998684,\"Y\":1515.2320306469073},{\"X\":1610.2775799367409,\"Y\":1466.1628359305932},{\"X\":1552.4607902322687,\"Y\":1282.8086880055864},{\"X\":1643.0381956219007,\"Y\":1188.1040744916254},{\"X\":1602.0874222379953,\"Y\":956.38843672659823},{\"X\":2270.9500843947426,\"Y\":888.236777039212}]}\":\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":1320.8080073817982,\"Y\":2098.7228279467809},{\"X\":2638.8095603938218,\"Y\":2098.7228279467809},{\"X\":2638.8095603938218,\"Y\":1762.1521804845643},{\"X\":1917.149692622299,\"Y\":1762.1521804845645},{\"X\":1917.149692622299,\"Y\":1455.7865959353185},{\"X\":2707.95062140755,\"Y\":1455.7865959353185},{\"X\":2656.0948180923424,\"Y\":765.38527244931083},{\"X\":1434.65131500248,\"Y\":857.12746920098},{\"X\":1320.8080073817982,\"Y\":2098.7228279467809}]}\",\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":2164.4780645306982,\"Y\":1348.9419879920015},{\"X\":2320.0910154773919,\"Y\":1348.9419879920015},{\"X\":2472.97390882123,\"Y\":1348.9419879920015}]}\":\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":1320.8080073817982,\"Y\":2098.7228279467809},{\"X\":2638.8095603938218,\"Y\":2098.7228279467809},{\"X\":2638.8095603938218,\"Y\":1762.1521804845643},{\"X\":1917.149692622299,\"Y\":1762.1521804845645},{\"X\":1917.149692622299,\"Y\":1455.7865959353185},{\"X\":2707.95062140755,\"Y\":1455.7865959353185},{\"X\":2656.0948180923424,\"Y\":765.38527244931083},{\"X\":1434.65131500248,\"Y\":857.12746920098},{\"X\":1320.8080073817982,\"Y\":2098.7228279467809}]}\",\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":2183.5884375310434,\"Y\":1253.5296631366382},{\"X\":2361.0417888612956,\"Y\":1253.5296631366382},{\"X\":2533.0350400956595,\"Y\":1160.8434093236492}]}\":\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":1320.8080073817982,\"Y\":2098.7228279467809},{\"X\":2638.8095603938218,\"Y\":2098.7228279467809},{\"X\":2638.8095603938218,\"Y\":1762.1521804845643},{\"X\":1917.149692622299,\"Y\":1762.1521804845645},{\"X\":1917.149692622299,\"Y\":1455.7865959353185},{\"X\":2707.95062140755,\"Y\":1455.7865959353185},{\"X\":2656.0948180923424,\"Y\":765.38527244931083},{\"X\":1434.65131500248,\"Y\":857.12746920098},{\"X\":1320.8080073817982,\"Y\":2098.7228279467809}]}\",\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":2210.8889531203131,\"Y\":1111.7742146073351},{\"X\":2478.43400891712,\"Y\":1057.2528907364913}]}\":\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":1320.8080073817982,\"Y\":2098.7228279467809},{\"X\":2638.8095603938218,\"Y\":2098.7228279467809},{\"X\":2638.8095603938218,\"Y\":1762.1521804845643},{\"X\":1917.149692622299,\"Y\":1762.1521804845645},{\"X\":1917.149692622299,\"Y\":1455.7865959353185},{\"X\":2707.95062140755,\"Y\":1455.7865959353185},{\"X\":2656.0948180923424,\"Y\":765.38527244931083},{\"X\":1434.65131500248,\"Y\":857.12746920098},{\"X\":1320.8080073817982,\"Y\":2098.7228279467809}]}\"}";

            #endregion
            var dic =
                JsonConvert.DeserializeObject<Dictionary<CADModel.SerializeModel, CADModel.SerializeModel>>(dicStr);

            foreach (var model in dic)
            {
                var outLine = new Polyline();
                for (int i = 0; i < model.Value.Point2ds.Count(); i++)
                {
                    var point=new Point2d(model.Value.Point2ds[i].X, model.Value.Point2ds[i].Y);
                    outLine.AddVertexAt(i, point, 0,0,0);
                }
                var inLine = new Polyline();
                for (int i = 0; i < model.Key.Point2ds.Count(); i++)
                {
                    var point = new Point2d(model.Key.Point2ds[i].X, model.Key.Point2ds[i].Y);
                    inLine.AddVertexAt(i, point, 0, 0, 0);
                }
                Assert.AreEqual(true,  BaseTools.IsInside(outLine, inLine));
            }
        }
        [Test]
        public void InLineFalse()
        {
            #region MyRegion
            var dicStr = "{\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":4746.9523625076581,\"Y\":1073.9512371660812},{\"X\":4746.9523625076581,\"Y\":598.2944679625989},{\"X\":5237.8274504960755,\"Y\":563.49031459149865},{\"X\":5388.8659484062482,\"Y\":946.33600813871271},{\"X\":4746.9523625076581,\"Y\":1073.9512371660812}]}\":\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":3747.0055346376212,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":1762.1521804845643},{\"X\":4343.3472198781219,\"Y\":1762.1521804845645},{\"X\":4343.3472198781219,\"Y\":1455.7865959353185},{\"X\":5134.1481486633729,\"Y\":1455.7865959353185},{\"X\":5082.2923453481653,\"Y\":765.38527244931083},{\"X\":3860.8488422583032,\"Y\":857.12746920098},{\"X\":3747.0055346376212,\"Y\":2098.7228279467809}]}\",\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":3747.0055346376212,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":2098.7228279467809},{\"X\":5349.6344309168962,\"Y\":2377.8358739690084}]}\":\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":3747.0055346376212,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":1762.1521804845643},{\"X\":4343.3472198781219,\"Y\":1762.1521804845645},{\"X\":4343.3472198781219,\"Y\":1455.7865959353185},{\"X\":5134.1481486633729,\"Y\":1455.7865959353185},{\"X\":5082.2923453481653,\"Y\":765.38527244931083},{\"X\":3860.8488422583032,\"Y\":857.12746920098},{\"X\":3747.0055346376212,\"Y\":2098.7228279467809}]}\",\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":3996.5573922835792,\"Y\":2098.7228279467809},{\"X\":3996.5573922835792,\"Y\":2355.4666385200189}]}\":\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":3747.0055346376212,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":1762.1521804845643},{\"X\":4343.3472198781219,\"Y\":1762.1521804845645},{\"X\":4343.3472198781219,\"Y\":1455.7865959353185},{\"X\":5134.1481486633729,\"Y\":1455.7865959353185},{\"X\":5082.2923453481653,\"Y\":765.38527244931083},{\"X\":3860.8488422583032,\"Y\":857.12746920098},{\"X\":3747.0055346376212,\"Y\":2098.7228279467809}]}\",\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":4117.5278597548931,\"Y\":1993.0850294184756},{\"X\":4117.5278597548931,\"Y\":2427.0481867846993}]}\":\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":3747.0055346376212,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":1762.1521804845643},{\"X\":4343.3472198781219,\"Y\":1762.1521804845645},{\"X\":4343.3472198781219,\"Y\":1455.7865959353185},{\"X\":5134.1481486633729,\"Y\":1455.7865959353185},{\"X\":5082.2923453481653,\"Y\":765.38527244931083},{\"X\":3860.8488422583032,\"Y\":857.12746920098},{\"X\":3747.0055346376212,\"Y\":2098.7228279467809}]}\",\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":4471.63168695652,\"Y\":1843.5445174544757},{\"X\":4471.63168695652,\"Y\":1320.5392356068205}]}\":\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":3747.0055346376212,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":1762.1521804845643},{\"X\":4343.3472198781219,\"Y\":1762.1521804845645},{\"X\":4343.3472198781219,\"Y\":1455.7865959353185},{\"X\":5134.1481486633729,\"Y\":1455.7865959353185},{\"X\":5082.2923453481653,\"Y\":765.38527244931083},{\"X\":3860.8488422583032,\"Y\":857.12746920098},{\"X\":3747.0055346376212,\"Y\":2098.7228279467809}]}\",\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":4581.4544003288938,\"Y\":1857.603800267334},{\"X\":4581.4544003288938,\"Y\":1531.4284648694684}]}\":\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":3747.0055346376212,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":1762.1521804845643},{\"X\":4343.3472198781219,\"Y\":1762.1521804845645},{\"X\":4343.3472198781219,\"Y\":1455.7865959353185},{\"X\":5134.1481486633729,\"Y\":1455.7865959353185},{\"X\":5082.2923453481653,\"Y\":765.38527244931083},{\"X\":3860.8488422583032,\"Y\":857.12746920098},{\"X\":3747.0055346376212,\"Y\":2098.7228279467809}]}\",\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":4665.9334106153365,\"Y\":1877.2867962053342},{\"X\":4665.9334106153365,\"Y\":1741.4990174855957}]}\":\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":3747.0055346376212,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":1762.1521804845643},{\"X\":4343.3472198781219,\"Y\":1762.1521804845645},{\"X\":4343.3472198781219,\"Y\":1455.7865959353185},{\"X\":5134.1481486633729,\"Y\":1455.7865959353185},{\"X\":5082.2923453481653,\"Y\":765.38527244931083},{\"X\":3860.8488422583032,\"Y\":857.12746920098},{\"X\":3747.0055346376212,\"Y\":2098.7228279467809}]}\",\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":4704.1771537638833,\"Y\":1762.1521804845643},{\"X\":4704.1771537638833,\"Y\":1455.7865959353185}]}\":\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":3747.0055346376212,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":1762.1521804845643},{\"X\":4343.3472198781219,\"Y\":1762.1521804845645},{\"X\":4343.3472198781219,\"Y\":1455.7865959353185},{\"X\":5134.1481486633729,\"Y\":1455.7865959353185},{\"X\":5082.2923453481653,\"Y\":765.38527244931083},{\"X\":3860.8488422583032,\"Y\":857.12746920098},{\"X\":3747.0055346376212,\"Y\":2098.7228279467809}]}\",\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":4743.1784126880884,\"Y\":1835.0131477940204},{\"X\":4821.8608573803285,\"Y\":1835.0131477940204},{\"X\":4821.8608573803285,\"Y\":1681.4492373932635},{\"X\":4741.3901804597917,\"Y\":1681.4492373932635},{\"X\":4741.3901804597917,\"Y\":1835.0131477940204}]}\":\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":3747.0055346376212,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":1762.1521804845643},{\"X\":4343.3472198781219,\"Y\":1762.1521804845645},{\"X\":4343.3472198781219,\"Y\":1455.7865959353185},{\"X\":5134.1481486633729,\"Y\":1455.7865959353185},{\"X\":5082.2923453481653,\"Y\":765.38527244931083},{\"X\":3860.8488422583032,\"Y\":857.12746920098},{\"X\":3747.0055346376212,\"Y\":2098.7228279467809}]}\",\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":4871.2744754060368,\"Y\":1762.1521804845643},{\"X\":4924.9606824604853,\"Y\":1762.1521804845643},{\"X\":4924.9606824604853,\"Y\":1679.8205473451562},{\"X\":4861.3849161177859,\"Y\":1679.8205473451562},{\"X\":4871.2744754060368,\"Y\":1762.1521804845643}]}\":\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":3747.0055346376212,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":1762.1521804845643},{\"X\":4343.3472198781219,\"Y\":1762.1521804845645},{\"X\":4343.3472198781219,\"Y\":1455.7865959353185},{\"X\":5134.1481486633729,\"Y\":1455.7865959353185},{\"X\":5082.2923453481653,\"Y\":765.38527244931083},{\"X\":3860.8488422583032,\"Y\":857.12746920098},{\"X\":3747.0055346376212,\"Y\":2098.7228279467809}]}\",\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":4950.3909950414927,\"Y\":1823.7152678396405},{\"X\":4950.3909950414927,\"Y\":1424.2854792542703},{\"X\":5014.0824421752368,\"Y\":1424.2854792542703},{\"X\":5014.0824421752377,\"Y\":1823.7152678396405},{\"X\":4950.3909950414927,\"Y\":1823.7152678396405}]}\":\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":3747.0055346376212,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":1762.1521804845643},{\"X\":4343.3472198781219,\"Y\":1762.1521804845645},{\"X\":4343.3472198781219,\"Y\":1455.7865959353185},{\"X\":5134.1481486633729,\"Y\":1455.7865959353185},{\"X\":5082.2923453481653,\"Y\":765.38527244931083},{\"X\":3860.8488422583032,\"Y\":857.12746920098},{\"X\":3747.0055346376212,\"Y\":2098.7228279467809}]}\",\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":4755.4115656825561,\"Y\":1634.6290533880674},{\"X\":5324.7514413889367,\"Y\":1634.6290533880674}]}\":\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":3747.0055346376212,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":1762.1521804845643},{\"X\":4343.3472198781219,\"Y\":1762.1521804845645},{\"X\":4343.3472198781219,\"Y\":1455.7865959353185},{\"X\":5134.1481486633729,\"Y\":1455.7865959353185},{\"X\":5082.2923453481653,\"Y\":765.38527244931083},{\"X\":3860.8488422583032,\"Y\":857.12746920098},{\"X\":3747.0055346376212,\"Y\":2098.7228279467809}]}\",\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":4790.6284641203856,\"Y\":1576.0199062776337},{\"X\":4902.1486450251505,\"Y\":1576.0199062776337}]}\":\"{\"CadType\":1,\"CadTypeStr\":\"PolyLine\",\"Point3ds\":null,\"Point2ds\":[{\"X\":3747.0055346376212,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":2098.7228279467809},{\"X\":5065.0070876496447,\"Y\":1762.1521804845643},{\"X\":4343.3472198781219,\"Y\":1762.1521804845645},{\"X\":4343.3472198781219,\"Y\":1455.7865959353185},{\"X\":5134.1481486633729,\"Y\":1455.7865959353185},{\"X\":5082.2923453481653,\"Y\":765.38527244931083},{\"X\":3860.8488422583032,\"Y\":857.12746920098},{\"X\":3747.0055346376212,\"Y\":2098.7228279467809}]}\"}";

            #endregion
            var dic =
                JsonConvert.DeserializeObject<Dictionary<CADModel.SerializeModel, CADModel.SerializeModel>>(dicStr);

            foreach (var model in dic)
            {
                var outLine = new Polyline();
                for (int i = 0; i < model.Value.Point2ds.Count(); i++)
                {
                    var point = new Point2d(model.Value.Point2ds[i].X, model.Value.Point2ds[i].Y);
                    outLine.AddVertexAt(i, point, 0, 0, 0);
                }
                var inLine = new Polyline();
                for (int i = 0; i < model.Key.Point2ds.Count(); i++)
                {
                    var point = new Point2d(model.Key.Point2ds[i].X, model.Key.Point2ds[i].Y);
                    inLine.AddVertexAt(i, point, 0, 0, 0);
                }
                Assert.AreEqual(false, BaseTools.IsInside(outLine, inLine));
            }
        }
    }
}