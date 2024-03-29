﻿using System;
using System.Collections.Generic;
using System.Linq;

#if ZWCAD
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.Colors;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using ZwSoft.ZwCAD.Runtime;
#elif AutoCAD
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
#endif
namespace AcDotNetTool
{
    /// <summary>
    /// 数据库相关处理
    /// </summary>
    public static class DataBaseTools
    {
        #region 事务
        /// <summary>
        /// 事务内执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ent"></param>
        /// <param name="excuteAction"></param>
        /// <returns></returns>
        public static T TransactionExcute<T>(this T ent, Action<T> excuteAction) where T : DBObject
        {
            var db = ent.Database;
            var trans = db?.TransactionManager.StartTransaction();
            var ent1 = trans?.GetObject(ent.ObjectId, OpenMode.ForWrite) as T;
            ent1 = ent1 ?? ent;
            excuteAction(ent1);
            trans?.Commit();
            trans?.Dispose();
            return ent;
        }
        #endregion


        #region 获得数据库

        /// <summary>
        /// 当前工作的数据库
        /// </summary>
        /// <returns></returns>
        public static Database WorkingDataBase()
        {
            return HostApplicationServices.WorkingDatabase;
        }

        /// <summary>
        /// 当前活动文档数据库
        /// </summary>
        /// <returns></returns>
        public static Database DocumentDatabase()
        {
            return Application.DocumentManager.MdiActiveDocument.Database;
        }

        #endregion

        public static Editor DocumentEditor()
        {
            return Application.DocumentManager.MdiActiveDocument.Editor;
        }

        #region 获得对象

        /// <summary>
        /// 由对象ObjectId获得对象
        /// </summary>
        /// <param name="id">ObjectId</param>
        /// <param name="mode">打开模式</param>
        /// <param name="db">数据库</param>
        /// <returns></returns>
        public static DBObject GetObject(ObjectId id, OpenMode mode, Database db)
        {
            DBObject Obj;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Obj = tr.GetObject(id, mode) as DBObject;
                tr.Commit();
            }

            return Obj;
        }
        #endregion


        #region 删除对象
        /// <summary>
        /// 删除单个对象
        /// </summary>
        /// <param name="obj">要删除对象</param>
        public static void Remove(DBObject obj)
        {
            Database db = obj.Database;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                var ent = trans.GetObject(obj.ObjectId, OpenMode.ForWrite) as Entity;
                ent.Erase();
                trans.Commit();
            }
        }
        /// <summary>
        /// 删除单个对象
        /// </summary>
        /// <param name="obj">要删除对象</param>
        public static bool TryRemove(DBObject obj)
        {
            Database db = obj.Database;
            bool result = true;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                try
                {
                    var ent = trans.GetObject(obj.ObjectId, OpenMode.ForWrite) as Entity;
                    ent.Erase();
                    trans.Commit();
                }
                catch (System.Exception)
                {
                    result = false;
                }
            }
            return result;
        }
        /// <summary>
        /// 删除ObjectId集合中的对象
        /// </summary>
        /// <param name="ids"></param>
        public static void Remove(ObjectIdCollection ids)
        {
            if (ids.Count == 0)
            {
                return;
            }

            //获得所选对象第一个ID所在的数据库
            Database db = ids[0].OriginalDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                Entity ent;
                foreach (ObjectId id in ids)
                {
                    ent = trans.GetObject(id, OpenMode.ForWrite) as Entity;
                    if (ent != null)
                        ent.Erase();
                }

                trans.Commit();
            }
        }

        #endregion


        #region 数据库克隆
        /// <summary>
        /// 克隆数据库对象
        /// </summary>
        /// <param name="objs">被克隆的对象</param>
        /// <param name="newDatabase">克隆的数据库</param>
        /// <returns>返回克隆对象的id关联(原id,新id)</returns>
        public static Dictionary<long, long> Clone(DBObjectCollection objs, Database newDatabase)
        {
            var idCollection = new ObjectIdCollection();
            var handleObjectIdDic = new Dictionary<long, ObjectId>();

            foreach (DBObject obj in objs)
            {
                var objectId = obj.Id;
                idCollection.Add(objectId);
                handleObjectIdDic[obj.Handle.Value] = objectId;
            }


            var result = new Dictionary<long, long>();
            var dic = Clone(idCollection, newDatabase);
            using (var trans = newDatabase.TransactionManager.StartTransaction())
            {
                foreach (KeyValuePair<ObjectId, ObjectId> keyValuePair in dic)
                {
                    var obj = trans.GetObject(keyValuePair.Value, OpenMode.ForRead);
                    var newHandle = obj.Handle.Value;
                    var oldHandle = handleObjectIdDic.First(x => x.Value == keyValuePair.Key).Key;
                    result[oldHandle] = newHandle;
                }
            }
            return result;
        }
        /// <summary>
        /// 克隆数据库对象
        /// </summary>
        /// <param name="handles">被克隆的对象</param>
        /// <param name="oldDatabase">被克隆数据库</param>
        /// <param name="newDatabase">克隆的数据库</param>
        /// <returns>返回克隆对象的id关联(原id,新id)</returns>
        public static Dictionary<long, long> Clone(IEnumerable<long> handles, Database oldDatabase, Database newDatabase)
        {
            var idCollection = new ObjectIdCollection();
            var handleObjectIdDic = new Dictionary<long, ObjectId>();
            using (var trans = oldDatabase.TransactionManager.StartTransaction())
            {
                foreach (var handle in handles)
                {
                    var objectId = oldDatabase.GetObjectId(false, new Handle(handle), 0);
                    idCollection.Add(objectId);
                    handleObjectIdDic[handle] = objectId;
                }
            }

            var result = new Dictionary<long, long>();
            var dic = Clone(idCollection, newDatabase);
            using (var trans = newDatabase.TransactionManager.StartTransaction())
            {
                foreach (KeyValuePair<ObjectId, ObjectId> keyValuePair in dic)
                {
                    var obj = trans.GetObject(keyValuePair.Value, OpenMode.ForRead);
                    var newHandle = obj.Handle.Value;
                    var oldHandle = handleObjectIdDic.First(x => x.Value == keyValuePair.Key).Key;
                    result[oldHandle] = newHandle;
                }
            }
            return result;
        }
        /// <summary>
        /// 克隆数据库对象
        /// </summary>
        /// <param name="idCollection">被克隆的对象</param>
        /// <param name="newDatabase">克隆的数据库</param>
        /// <returns>返回克隆对象的id关联(原id,新id)</returns>
        public static Dictionary<ObjectId, ObjectId> Clone(ObjectIdCollection idCollection, Database newDatabase)
        {
            ObjectId idBtr;
            var db = idCollection[0].Database;
            var map = new IdMapping();
            using (var trans = newDatabase.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)trans.GetObject(newDatabase.BlockTableId, OpenMode.ForRead);
                var btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                idBtr = btr.ObjectId;
                trans.Commit();
            }
            var dic = new Dictionary<ObjectId, ObjectId>();
            db.WblockCloneObjects(idCollection, idBtr, map, DuplicateRecordCloning.Replace, false);
            foreach (ObjectId id in idCollection)
            {
                dic[id] = map[id].Value;
            }
            return dic;
        }
        /// <summary>
        /// 数据库克隆
        /// </summary>
        /// <param name="idCollection">克隆的对象ID集合</param>
        /// <param name="fileName">克隆到的文件名</param>
        public static void Clone(ObjectIdCollection idCollection, string fileName)
        {
            var ndb = new Database(true, true);
            Clone(idCollection, ndb);
            ndb.SaveAs(fileName, DwgVersion.Current);
        }
        #endregion


        #region 获取块表
        /// <summary>
        /// 获取块表
        /// </summary>
        /// <param name="db">数据库</param>
        /// <returns></returns>
        public static BlockTable BlockTable(this Database db)
        {
            BlockTable bt;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                bt = tr.GetObject(db.BlockTableId, OpenMode.ForWrite) as BlockTable;
                tr.Commit();
            }

            return bt;
        }
        #endregion


        #region 获取块表记录

        /// <summary>
        /// 由块表记录名获得块表记录
        /// </summary>
        /// <param name="btrName">块表记录名</param>
        /// <param name="db">数据库</param>
        /// <returns></returns>
        public static BlockTableRecord GetBlock(string btrName, Database db)
        {
            BlockTableRecord block = new BlockTableRecord();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                block = tr.GetObject(bt[btrName], OpenMode.ForRead) as BlockTableRecord;
                tr.Commit();
            }

            return block;
        }

        #endregion


        #region 添加对象到块表记录
        /// <summary>
        /// 将一个实体添加到当前空间
        /// </summary>
        /// <param name="ent">对象</param>
        /// <param name="db">数据库</param>
        /// <returns></returns>
        public static ObjectId AddIn(this Entity ent)
        {
            var db = DocumentDatabase();
            return AddIn(ent, db);
        }
        /// <summary>
        /// 将一个实体添加到当前空间
        /// </summary>
        /// <param name="ent">对象</param>
        /// <param name="db">数据库</param>
        /// <returns></returns>
        public static ObjectId AddIn(this Entity ent, Database db)
        {
            ObjectId id;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                id = ((BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite, false)).AppendEntity(ent);
                tr.AddNewlyCreatedDBObject(ent, true);
                tr.Commit();
            }

            return id;
        }
        /// <summary>
        /// 将一个实体添加到块表记录
        /// </summary>
        /// <param name="ent">对象</param>
        /// <param name="block">块定义</param>
        /// <returns></returns>
        public static ObjectId AddEntToBlock(this Entity ent, BlockTableRecord block)
        {
            Database db = block.Database;
            return AddIn(ent, db);
        }

        #endregion


        #region  添加块表记录(图块)到块表

        /// <summary>
        /// 将块表记录加入到块表中
        /// </summary>
        /// <returns></returns>
        public static ObjectId AddIn(BlockTableRecord btr, Database db)
        {
            ObjectId id = new ObjectId();
            using (Transaction transaction = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = transaction.GetObject(db.BlockTableId, OpenMode.ForWrite) as BlockTable;
                id = bt.Add(btr);
                transaction.AddNewlyCreatedDBObject(btr, true);
                transaction.Commit();
            }

            return id;
        }

        #endregion


        #region 模型空间

        /// <summary>
        /// 获得模型空间ObjectId
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static ObjectId ModelSpaceId(this Database db)
        {
            return SymbolUtilityServices.GetBlockModelSpaceId(db);
        }
        /// <summary>
        /// 将一个图形对象加入到指定的Database的模型空间
        /// </summary>
        /// <param name="ent">实体对象</param>
        /// <returns></returns>
        public static ObjectId AddToModelSpace(this Entity ent)
        {
            return AddToModelSpace(ent, DocumentDatabase());
        }
        /// <summary>
        /// 将一个图形对象加入到指定的Database的模型空间
        /// </summary>
        /// <param name="ent">实体对象</param>
        /// <param name="db">数据库</param>
        /// <returns></returns>
        public static ObjectId AddToModelSpace(this Entity ent, Database db)
        {
            ObjectId entId;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr =
                    (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                entId = btr.AppendEntity(ent);
                trans.AddNewlyCreatedDBObject(ent, true);
                trans.Commit();
            }

            return entId;
        }
        /// <summary>
        /// 将一个图形对象加入到指定的Database的模型空间
        /// </summary>
        /// <param name="ents">实体对象</param>
        /// <returns></returns>
        public static IEnumerable<ObjectId> AddToModelSpace(this IEnumerable<Entity> ents)
        {
            return AddToModelSpace(ents, DocumentDatabase());
        }
        /// <summary>
        /// 将一个图形对象加入到指定的Database的模型空间
        /// </summary>
        /// <param name="ents">实体对象</param>
        /// <param name="db">数据库</param>
        /// <returns></returns>
        public static IEnumerable<ObjectId> AddToModelSpace(this IEnumerable<Entity> ents, Database db)
        {
            var entIds = new List<ObjectId>();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                foreach (var ent in ents)
                {
                    BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord btr =
                        (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                    var entId = btr.AppendEntity(ent);
                    entIds.Add(entId);
                    trans.AddNewlyCreatedDBObject(ent, true);
                }

                trans.Commit();
            }

            return entIds;
        }
        /// <summary>
        /// 将一组图形对象加入到指定的Database的模型空间
        /// </summary>
        /// <param name="entCollection">要添加的对象集合</param>
        /// <param name="db">数据库</param>
        /// <returns></returns>
        public static ObjectIdCollection AddToModelSpace(this Database db, DBObjectCollection entCollection)
        {
            ObjectIdCollection objIds = new ObjectIdCollection();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr =
                    (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                foreach (DBObject obj in entCollection)
                {
                    Entity ent = obj as Entity;
                    if (ent != null)
                    {
                        objIds.Add(btr.AppendEntity(ent));
                        trans.AddNewlyCreatedDBObject(ent, true);
                    }
                }

                trans.Commit();
            }

            return objIds;
        }

        /// <summary>
        /// 将指定的块定义变成块参照添加到指定模型空间
        /// </summary>
        /// <param name="block">块定义</param>
        /// <param name="pt">插入点</param>
        /// <param name="db">数据库</param>
        /// <returns></returns>
        public static ObjectId AddToModelSpace(BlockTableRecord block, Point3d pt, Database db)
        {
            ObjectId blkrfid = new ObjectId();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelspace =
                    trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                BlockReference br = new BlockReference(pt, block.ObjectId); // 通过块定义添加块参照
                blkrfid = modelspace.AppendEntity(br); //把块参照添加到块表记录
                trans.AddNewlyCreatedDBObject(br, true); // 通过事务添加块参照到数据库
                foreach (ObjectId id in block)
                {
                    if (id.ObjectClass.Equals(RXClass.GetClass(typeof(AttributeDefinition))))
                    {
                        AttributeDefinition ad = trans.GetObject(id, OpenMode.ForRead) as AttributeDefinition;
                        AttributeReference ar =
                            new AttributeReference(ad.Position, ad.TextString, ad.Tag, new ObjectId());
                        br.AttributeCollection.AppendAttribute(ar);
                    }
                }

                trans.Commit();
            }

            return blkrfid;
        }
        #endregion


        #region 获得层表

        /// <summary>
        /// 获得层表
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static LayerTable GetLayerTable(this Database db)
        {
            LayerTable layertable;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                layertable = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForWrite);
            }

            return layertable;
        }

        #endregion


        #region 获得层表记录

        /// <summary>
        /// 根据图层名获得图层(没有删除标记的)
        /// </summary>
        /// <param name="name">图层名</param>
        /// <param name="db">数据库</param>
        /// <returns></returns>
        public static LayerTableRecord GetLayer(this Database db, string name)
        {
            LayerTableRecord layer = new LayerTableRecord();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTableRecord layer0 = new LayerTableRecord();
                LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead);
                if (lt.Has(name))
                {
                    layer0 = trans.GetObject(lt[name], OpenMode.ForRead) as LayerTableRecord;
                    if (!layer0.IsErased)
                        layer = layer0;
                }

                trans.Commit();
            }

            return layer;
        }

        /// <summary>
        /// 获取当前图层ObjectId
        /// </summary>
        /// <param name="db">数据库</param>
        /// <returns></returns>
        public static LayerTableRecord GetCurrentLayer(this Database db)
        {
            LayerTableRecord layer = new LayerTableRecord();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                layer = tr.GetObject(db.Clayer, OpenMode.ForRead) as LayerTableRecord;
            }

            return layer;
        }

        #endregion


        #region 设置当前图层

        /// <summary>
        /// 设置当前层
        /// </summary>
        /// <param name="layer">要设置为当前的图层</param>
        /// <param name="db">数据库</param>
        public static void SetCurrentLayer(LayerTableRecord layer, Database db)
        {
            if (layer.ObjectId != ObjectId.Null)
                db.Clayer = layer.ObjectId;
        }
        #endregion


        #region 新建层表记录(图层)

        /// <summary>
        /// 新建一个给定名字的图层
        /// </summary>
        /// <param name="layerName">新增图层名</param>
        /// <param name="db">数据库</param>
        /// <returns></returns>
        public static ObjectId AddIn(string layerName, Database db)
        {
            ObjectId layerId = ObjectId.Null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForWrite);
                if (!lt.Has(layerName))
                {
                    LayerTableRecord ltr = new LayerTableRecord();
                    ltr.Name = layerName;
                    layerId = lt.Add(ltr);
                    trans.AddNewlyCreatedDBObject(ltr, true);
                }

                trans.Commit();
            }

            return layerId;
        }

        /// <summary>
        /// 建立指定名字，颜色的图层
        /// </summary>
        /// <param name="layerName">新增图层名</param>
        /// <param name="colorIndex">颜色值</param>
        /// <param name="db">数据库</param>
        /// <returns></returns>
        public static ObjectId AddIn(string layerName, short colorIndex, Database db)
        {
            short colorIndex1 = (short)(colorIndex % 256); //防止输入的颜色超出256
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForWrite);
                ObjectId layerId = ObjectId.Null;
                if (lt.Has(layerName) == false)
                {
                    LayerTableRecord ltr = new LayerTableRecord();
                    ltr.Name = layerName;
                    ltr.Color = Color.FromColorIndex(ColorMethod.ByColor, colorIndex1);
                    layerId = lt.Add(ltr);
                    trans.AddNewlyCreatedDBObject(ltr, true);
                }

                trans.Commit();
                return layerId;
            }
        }

        #endregion


        #region 删除层表记录(图层)

        /// <summary>
        /// 删除指定名字的图层
        /// </summary>
        /// <param name="LayerName"></param>
        /// <param name="db"></param>
        public static void RemoveLayer(this Database db, string layerName)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForWrite);
                LayerTableRecord currentLayer = (LayerTableRecord)trans.GetObject(db.Clayer, OpenMode.ForRead);
                if (currentLayer.Name.ToLower() == layerName.ToLower())
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n不能删除当前层");
                else
                {
                    LayerTableRecord ltr = new LayerTableRecord();
                    if (lt.Has(layerName))
                    {
                        ltr = trans.GetObject(lt[layerName], OpenMode.ForWrite) as LayerTableRecord;
                        if (ltr.IsErased)
                            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n此层已经被删除");
                        else
                        {
                            ObjectIdCollection idCol = new ObjectIdCollection();
                            idCol.Add(ltr.ObjectId);
                            db.Purge(idCol);
                            if (idCol.Count == 0)
                                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n不能删除包含对象的图层");
                            else
                                ltr.Erase();
                        }
                    }
                    else
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n没有此图层");
                }

                trans.Commit();
            }
        }

        #endregion


        #region 删除与图层关联的对象
        /// <summary>
        /// 删除与图层关联的对象
        /// </summary>
        /// <param name="layer">图层</param>
        /// <param name="db">数据库</param>
        public static void RemoveAllEntFromLayer(LayerTableRecord layer, Database db)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForWrite);
                foreach (ObjectId btrid in bt) //遍历块表
                {
                    BlockTableRecord btr = (BlockTableRecord)trans.GetObject(btrid, OpenMode.ForWrite);
                    foreach (ObjectId eid in btr) //遍历块表记录
                    {
                        Entity ent = trans.GetObject(eid, OpenMode.ForWrite) as Entity;
                        if (ent != null)
                        {
                            if (ent.LayerId == layer.ObjectId)
                                ent.Erase();
                        }
                    }
                }

                trans.Commit();
            }
        }
        #endregion


        #region 命名对象词典

        /// <summary>
        /// 将一个对象加到命名对象词典
        /// </summary>
        /// <param name="dataObj">要添加到命名对象词典的对象</param>
        /// <param name="name">词典记录名</param>
        /// <param name="db">数据库</param>
        public static void AddObjToNOD(DBObject dataObj, string name, Database db)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary Dict = tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite) as DBDictionary;
                Dict.SetAt(name, dataObj);
                tr.Commit();
            }
        }
        /// <summary>
        /// 获得命名对象扩展词典中的对象
        /// </summary>
        /// <param name="name">词典记录名</param>
        /// <param name="db">词典数据库</param>
        /// <returns></returns>
        public static DBObject GetObjFromNod(this Database db, string name)
        {
            DBObject obj = null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary Dict = tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite) as DBDictionary;
                obj = tr.GetObject(Dict.GetAt(name), OpenMode.ForWrite);
                tr.Commit();
            }

            return obj;
        }
        #endregion


        #region 对象扩展词典

        /// <summary>
        /// 添加一个对象到对象扩展词典
        /// </summary>
        /// <param name="dataObj">要添加到扩展词典的对象</param>
        /// <param name="dbObject">词典对象的宿主</param>
        /// <param name="Name">词典记录名</param>
        public static void AddObjToExtensionDictionary(DBObject dataObj, DBObject dbObject, string Name)
        {
            Database db = dbObject.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                if (dbObject.ExtensionDictionary == new ObjectId())
                    dbObject.CreateExtensionDictionary();
                DBDictionary extensionDic =
                    (DBDictionary)tr.GetObject(dbObject.ExtensionDictionary, OpenMode.ForWrite, false);
                extensionDic.SetAt(Name, dataObj);
                tr.Commit();
            }
        }

        /// <summary>
        /// 从对象扩展词典读取数据对象
        /// </summary>
        /// <param name="dbObject">词典对象的宿主</param>
        /// <param name="name">词典记录名</param>
        public static DBObject GetObjFromExtensionDictionary(DBObject dbObject, string name)
        {
            DBObject obj = null;
            Database db = dbObject.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                if (dbObject.ExtensionDictionary != new ObjectId())
                {
                    DBDictionary extensionDic =
                        (DBDictionary)tr.GetObject(dbObject.ExtensionDictionary, OpenMode.ForWrite, false);
                    obj = tr.GetObject(extensionDic.GetAt(name), OpenMode.ForWrite);
                }

                tr.Commit();
            }

            return obj;
        }
        #endregion

        #region 读取文件
        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="fileName">文件全路径</param>
        /// <returns></returns>
        public static Database ReadFile(string fileName)
        {
            var database = new Database(false, false);
            database.ReadDwgFile(fileName, FileOpenMode.OpenForReadAndAllShare, true, null);
            return database;
        }
        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="fileName">文件全路径</param>
        /// <returns></returns>
        public static Database ReadDxfFile(string fileName)
        {
            var database = new Database(false, false);
            database.DxfIn(fileName, "dxflog.txt");
            return database;
        }


        /// <summary>
        /// 获取模型空间中的所有对象
        /// </summary>
        /// <param name="db">数据库</param>
        /// <returns></returns>
        public static IEnumerable<Entity> GetAllEntitiesInModel(Database db)
        {
            var list = new List<Entity>();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr =
                    (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                foreach (ObjectId objId in btr)
                {
                    var ent = trans.GetObject(objId, OpenMode.ForRead) as Entity;
                    if (ent != null)
                    {
                        list.Add(ent);
                    }
                }
            }

            return list;
        }
        #endregion
    }

}
