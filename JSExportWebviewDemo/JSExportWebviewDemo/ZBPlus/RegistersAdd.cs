using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using ZBServices.db.Tables.Comm;
using ZBServices.db.Tables.Produce;
using ZBServices.db.Tables.Store;
using ZBServices.flib.Produce.Material;
using ZBServices.ibll.Interfaces.produce.material;
using ZBServices.ibll.Interfaces.produce.material.datastructs;
using ZBServices.sdk.bill;
using ZBServices.sdk.flib;
using ZBServices.sdk.leftmenuframe;
using ZBServices.sdk.listview;
using ZBServices.sdk.orm;
using ZBServices.sdk.remind;
using ZBServices.ui;
using ZBServices.view.SYSN.mdl.comm;
using ZBServices.view.SYSN.mdl.produceV2;
using ZBServices.view.SYSN.mdl.produceV2.QualityControl;
using ZBServices.view.SYSN.mdl.relevance;
using ZBServices.view.SYSN.mdl.store;

namespace ZBServices.view.SYSN.view.produceV2.MaterialRegisters
{
    /// <summary>
    /// 登记添加
    (){}=;
    /// </summary>
    public class RegistersAdd : BillPage
    {
        private int FromKuinId = 0;//所选择入库单ID
        public readonly IMaterialRegHelper materialRegHelper = MaterialRegHelper.Instance;
        public List<MaterialRegStatusItem> materialRegStatusItems = null;

        public override void Bill_SetGlobalInformation(IBillGlobalInformation Ibill)
        {
            Ibill.Source.MainTable = "M2_MaterialRegisters";
            Ibill.Source.KeyField = "ID";

            #region 产品自定义设置
            var cplist = Ibill.ChildSources.Add("rglvw", "M2_MaterialRegisterLists", "ID", "MRID");
            cplist.ListType = 1;
            cplist.CustomOpenExcelOut = true;
            cplist.CustomInheritIdType = BillApplyTypeEnum.B16001_产品信息;
            cplist.Cols.Add("物料名称", "protitle", false);
            cplist.Cols.Add("物料编号", "order1");
            cplist.Cols.Add("物料型号", "type1");
            cplist.Cols.Add("单位", "unit", false);
            cplist.Cols.Add("所需数量", "sx");
            cplist.Cols.Add("本次登记数量", "num1", false);
            cplist.Cols.Add("成本单价", "cbprice");
            cplist.Cols.Add("成本总价", "cbmoney");
            cplist.Cols.Add("可用数量", "ky");
            cplist.Cols.Add("分配方式", "zy");
            cplist.Cols.Add("备注", "intro");
            cplist.Cols.Add("批号", "ph");
            cplist.Cols.Add("序列号", "xlh");
            cplist.Cols.Add("生产日期", "datesc");
            cplist.Cols.Add("有效日期", "dateyx");
            #endregion

            Ibill.Source.TitleField = "title";
            Ibill.ApproveInfo.Enable = true;
            Ibill.BindPower(SQLPowerTypeEnum.物料登记);
            Ibill.BillType = BillApplyTypeEnum.B55004_生产物料登记;

            Ibill.Name = "物料登记";
            Ibill.SignUrlInfo.AddUrl = "/SYSN/view/produceV2/MaterialRegisters/RegistersAdd.ashx";
            Ibill.SignUrlInfo.ModifyUrl = "/SYSN/view/produceV2/MaterialRegisters/RegistersAdd.ashx";
            Ibill.SignUrlInfo.DetailsUrl = "/SYSN/view/produceV2/MaterialRegisters/RegistersAdd.ashx";
            Ibill.SignUrlInfo.ListUrl = "/SYSN/view/produceV2/MaterialRegisters/RegisterList.ashx";
            Ibill.FunFeatures.OpenTempSave = true;
            Ibill.FunFeatures.OpenMdlPrint = true;

            Ibill.CustomType = BillCustomFieldType.CF_Auto;
            Ibill.ApproveInfo.OnApproveExecStatus = AutoReAnalysisRoomKuInfo;
            Ibill.SignFieldInfo.SNField = "title";
            Ibill.SignFieldInfo.SNField = "MOBH";
            Ibill.SignFieldInfo.AddDateField = "indate";
            Ibill.BindPower(SQLPowerTypeEnum.物料登记, "creator");

            Ibill.FunFeatures.OpenNotice = true;
            Ibill.FunFeatures.OpenChange = true;
            Ibill.CustomVisible.SetCustomTypeVisible(true, true, false, true, false);
        }

        #region  部分业务数据查询函数
        public static bool existsWorkAssginReg() { return SystemPowerClass.ExistsModule(18600); }

        /// <summary>获取初始化信息</summary>
        public string GetkyNumInfo(DataRow row)
        {
            decimal relust = 0.00m;
            if (materialRegStatusItems != null && materialRegStatusItems.Count() > 0)
            {
                int productid = ConvertHelper.ToInt(row["ProductID"]);
                int unit = ConvertHelper.ToInt(row["unit"]);
                var model = materialRegStatusItems.Where(p => p.ProductID == productid && p.UnitID == unit).FirstOrDefault();
                if (model != null)
                {
                    relust = model.EnableRoomKuNum;
                }
                else
                {
                    int ListID = ConvertHelper.ToInt(row["ListID"]);
                    int ForKuinID = ConvertHelper.ToInt(row["ForKuinID"]);
                    if (ListID == 0 && ForKuinID > 0)
                    {
                        FalseDeleteRegBill(Bill.Ord);
                        relust = MaterialRegHelper.Instance.GetRegisterOccupy(new int[] { ForKuinID }, productid, unit, "", "", GetDouble("n")).Sum(x => x.NowNum);
                        Sql.RollbackTrans();
                    }
                }
            }

            return relust.ToString();
        }

        //审批过程处理事件：自动释放库存
        public ReturnResult AutoReAnalysisRoomKuInfo(SQLClass Sql, IBillGlobalInformation bill, string Ords, int Status)
        {
            ReturnResult rt = new ReturnResult();
            if (Status == (int)BillApproveStatusEnum.A_审批未通过 || Status == (int)BillApproveStatusEnum.A_审批退回_或终止)
            {
                try
                {
                    MaterialRegHelper.Instance.ReAnalysisRoomKuInfo(Ords.ToIntList(true).ToList());
                }
                catch (Exception ex)
                {
                    rt.Error = "车间库存不足";
                    rt.Result = false;
                    throw;
                }
            }
            return rt;
        }

        /// <summary>
        /// 修改模式下，模拟删除本单据，再提取可用库存时则不包含当前单据库存
        /// </summary>
        /// <param name="billid"></param>
        public static void FalseDeleteRegBill(int billid)
        {
            SQLClass.CurrSql.BeginTrans();
            M2_MaterialRegisters.Update(
                x => new M2_MaterialRegisters { ForKuinID = -222 },
                x => x.ID == billid
            ).ExecAtOnce();

            M2_MaterialRegisterLists
            .Inner_Join<M2_RegisterOccupy>
             ((x, y) => x.MRID == billid && x.ID == y.MRLID)
             .Group_By(
                (x, y) => new { y.Kuoutlist2, sumNum = DBQuery.Sum(y.Num1) }
            )
            .Inner_Join<kuoutlist2>(
                (t1, z) => t1.Kuoutlist2 == z.Id
             ).Update<kuoutlist2>(
                (t1, z, m) => new kuoutlist2
                {
                    Numleft = z.Numleft + t1.sumNum
                },
                  (t1, z, m) => z.Id == m.Id
             ).ExecAtOnce();
        }

        /// <summary>
        /// 根据单据id获取明细数据源
        /// </summary>
        /// <param name="fromId">来源ID</param>
        /// <param name="fromType">1:派工2：委外</param>
        /// <returns></returns>
        public DataTable GetlvwDataById(int fromId, int fromType, bool Batch = false)
        {

            List<int> ids = new List<int>() { fromId };
            int[] kuinids = MaterialRegHelper.Instance.GetForKuInIds(ids, fromType).ToArray();
            int tabIndex = 0;
            if (fromType == 2)
            {
                tabIndex = 1;
            }
            if (kuinids == null || kuinids.Count() == 0)
            {
                kuinids = new int[] { -1 };
            }
            return GetLvwDataSqlTable(kuinids, tabIndex, Batch);

        }

        /// <summary>
        /// 获取登记列表信息
        /// </summary>
        /// <param name="KuInIds">入库单IDs</param>
        /// <param name="tabIndex">类型：0：派工单，1：委外单，2:修改，变更 </param>
        /// <param name="Batch">是否是批量 默认否</param>
        /// <returns></returns>
        public DataTable GetLvwDataSqlTable(int[] KuInIds, int tabIndex, bool Batch = false)
        {
            string mainsql = "";
            DataTable tb = null;
            List<MaterialRegStatusItem> lists = new List<MaterialRegStatusItem>();
            if (materialRegStatusItems == null)
            {
                lists = materialRegHelper.GetMaterialRegDetailsStatusList(KuInIds);
            }
            else
            {
                lists = materialRegStatusItems.Where(x => KuInIds.Contains(x.KuinID)).ToList();
            }
            if (lists != null && lists.Count() > 0)
            {
                var notOpenPublicConn = Sql.SessionConnection == null;
                if (notOpenPublicConn) { Sql.OpenPublicConnection(); }
                Sql.CreateSqlTableByDataTable("#MaterialRegStatusItem", Sql.ListConvertTable(lists));
                switch (tabIndex)
                {
                    case 0:
                        mainsql = @"SELECT cast(mri.KuinID as varchar(20)) + 'one' iden,z.ID bid,l.ID ListID,l.ProductID ProductID,p.title protitle
                                ,(case ISNULL(p.del, 99) when 1 then '' when 2 then '(已删除)' when 99 then '产品已彻底删除' end) deltitle
	                            ,p.order1
	                            ,p.type1
	                            ,l.unit,l.BomList,l.intro
                                ,z.ID billID
                                ,z.title billtitle
                                ,isnull(mri.NeedNum, 0) sx
	                            ,isnull(mri.RegedNum, 0) cbc
	                            ,isnull(mri.WaitRegNum, 0) num1,mri.EnableRoomKuNum ky
	                            ,z.WABH BH
                                ,(CASE z.[Status] WHEN 1 THEN '正常' WHEN 2 THEN '终止' END) [Status]
	                            ,zp.title ztitle
                                ,zp.order1 zorder1, ISNULL(mri.NeedBL, 1) bl,0 ID,NULL cbprice, NULL cbmoney,1 zy
	                            ,NULL ph, NULL xlh,NULL datesc, NULL dateyx,0 mapnum,'' zdmapnum
	                            ,NULL mapping,(select top 1 (case isnull(wa2.ptype,0) when 0 then 1 when 1 then 2 end) from dbo.M2_WorkAssigns wa2 where wa2.ID=l.waid) poTypeV,0 gys
                                ,ki.title as KuInTitle
                                ,z.NumMake as PGNum
                                ,zp.type1 as ProductModel
                                ,mri.KuinNum as KuInNum
                                ,mri.KuinRegCompletedNum
                                ,mri.KuinWaitRegNum as totalnum
                                ,mri.KuinID as ForKuinID
                                ,1 as canRk
                                ,ki.Date5 as date1
                                ,(select top 1 e.sort1 from dbo.sortonehy e where e.gate2 = 61 and e.ord=l.unit) unname
                                ,z.ProductID as zProductID
                                ,mri.NeedWastAge
                            FROM #MaterialRegStatusItem mri
                                INNER JOIN dbo.M2_WorkAssignLists l on mri.FromListId=l.Id
                                INNER JOIN dbo.M2_WorkAssigns z ON z.ID=mri.FromId
                                left JOIN dbo.product p ON l.ProductID = p.ord
                                LEFT JOIN dbo.product zp ON z.ProductID = zp.ord
                                INNER join dbo.KuIn ki on mri.KuinID=ki.Ord 
                            where mri.FromType=1";
                        if (Batch)
                        {
                            mainsql = @"select * from (SELECT cast(mri.KuinID as varchar(20)) + 'one' iden
	                                    ,0 bid,l.ID ListID,l.ID newbid
                                        ,l.title billtitle
                                        ,ki.title as KuInTitle
	                                    ,l.ProductID ProductID
	                                    ,zp.title protitle
	                                    ,(case ISNULL(zp.del, 99) when 1 then '' when 2 then '(已删除)' when 99 then '产品已彻底删除' end) deltitle
                                        ,zp.order1
                                        ,zp.type1
                                        ,l.unit,l.BomList
                                        ,isnull(mri.KuinNum, 0) sx
                                        ,isnull(mri.KuinRegCompletedNum, 0) cbc
                                        ,isnull(mri.KuinWaitRegNum, 0) num1
                                        ,isnull(mri.KuinNum,0) ky
                                        ,l.WABH BH
                                        ,(CASE l.[Status] WHEN 1 THEN '正常' WHEN 2 THEN '终止' END) [Status]
                                        ,1 bl,0 ID,NULL cbprice, NULL cbmoney,1 zy
                                        ,NULL ph, NULL xlh,NULL datesc, NULL dateyx,0 mapnum,'' zdmapnum
                                        ,NULL mapping,(case isnull(l.ptype,0) when 0 then 1 when 1 then 2 end) poTypeV,0 gys
                                        ,l.NumMake as PGNum
                                        ,mri.KuinID as ForKuinID
                                        ,1 as canRk
                                        ,ki.Date5 as date1
                                        ,(select top 1 e.sort1 from dbo.sortonehy e where e.gate2 = 61 and e.ord=l.unit) unname
                                        ,l.intro
                                        ,0 as NeedWastAge
                                    FROM (select distinct KuinID,FromId,KuinRegCompletedNum,KuinWaitRegNum,FromType,KuinNum from #MaterialRegStatusItem) mri
                                        INNER join dbo.KuIn ki on mri.KuinID=ki.Ord
                                        INNER JOIN dbo.M2_WorkAssigns l ON mri.FromId = l.ID
                                        LEFT JOIN dbo.product zp ON l.ProductID = zp.ord
                                    where mri.FromType=1
                                    union all
                                    SELECT cast(mri.KuinID as varchar(20)) + 'one' iden
	                                    ,z.ID bid,l.ID ListID,z.ID newbid
                                        ,z.title billtitle
                                        ,ki.title as KuInTitle
	                                    ,l.ProductID ProductID,p.title protitle
	                                    ,(case ISNULL(p.del, 99) when 1 then '' when 2 then '(已删除)' when 99 then '产品已彻底删除' end) deltitle
	                                    ,p.order1
	                                    ,p.type1
	                                    ,l.unit,l.BomList
	                                    ,isnull(mri.NeedNum, 0) sx
	                                    ,isnull(mri.RegedNum, 0) cbc
	                                    ,isnull(mri.WaitRegNum, 0) num1
                                        ,mri.EnableRoomKuNum ky
	                                    ,z.WABH BH
	                                    ,(CASE z.[Status] WHEN 1 THEN '正常' WHEN 2 THEN '终止' END) [Status]
	                                    ,ISNULL(mri.NeedBL, 1) bl,0 ID,NULL cbprice, NULL cbmoney,1 zy
	                                    ,NULL ph, NULL xlh,NULL datesc, NULL dateyx,0 mapnum,'' zdmapnum
	                                    ,NULL mapping,(select top 1 (case isnull(wa2.ptype,0) when 0 then 1 when 1 then 2 end) from dbo.M2_WorkAssigns wa2 where wa2.ID=l.waid) poTypeV,0 gys
	                                    ,z.NumMake as PGNum
	                                    ,mri.KuinID as ForKuinID
	                                    ,1 as canRk
	                                    ,ki.Date5 as date1
	                                    ,(select top 1 e.sort1 from dbo.sortonehy e where e.gate2 = 61 and e.ord=l.unit) unname
                                        ,l.intro
                                        ,mri.NeedWastAge
                                    FROM #MaterialRegStatusItem mri
                                        INNER join dbo.KuIn ki on mri.KuinID=ki.Ord
                                        INNER JOIN dbo.M2_WorkAssignLists l on mri.FromListId=l.Id
                                        INNER JOIN dbo.M2_WorkAssigns z ON z.ID=mri.FromId
                                        left JOIN dbo.product p ON l.ProductID = p.ord
                                    where mri.FromType=1) tblist order by tblist.date1,tblist.ForKuinID,tblist.bid asc";

                        }
                        break;
                    case 1:
                        mainsql = @"SELECT cast(mri.KuinID as varchar(20)) + 'two' iden,ool.ID bid,l.ID ListID,l.ProductID ProductID,p.title protitle,(case ISNULL(p.del,99) when 1 then '' when 2 then '(已删除)' when 99 then '产品已彻底删除' end) deltitle,p.order1
                                ,p.type1
                                ,l.unit,ool.BomListID BomList,l.remark intro
                                ,oo.ID billID
                                ,oo.title billtitle
                                ,isnull(mri.NeedNum, 0) sx
	                            ,isnull(mri.RegedNum, 0) cbc
	                            ,isnull(mri.WaitRegNum, 0) num1,mri.EnableRoomKuNum ky
                                ,oo.sn BH,(CASE oo.[Status] WHEN 1 THEN '正常' WHEN 2 THEN '终止' END) [Status],zp.title ztitle
                                , zp.order1 zorder1,ISNULL(mri.NeedBL,1) bl,0 ID,NULL cbprice,NULL cbmoney,1 zy,NULL ph,NULL xlh,NULL datesc,NULL dateyx,0 mapnum,'' zdmapnum,NULL mapping
                                ,(case isnull(oo.wwType,0) when 0 then 3 when 1 then 4 end) poTypeV
                                , oo.gys
                                ,ki.title as KuInTitle
                                ,ool.num1 as PGNum
                                ,zp.type1 as ProductModel
                                ,mri.KuinNum as KuInNum
                                ,mri.KuinRegCompletedNum
                                ,mri.KuinWaitRegNum as totalnum
                                ,mri.KuinID as ForKuinID
                                ,1 as canRk
                                ,ki.Date5 as date1
                                ,(select top 1 e.sort1 from dbo.sortonehy e where e.gate2 = 61 and e.ord=l.unit) unname
                                ,ool.ProductID as zProductID
                                ,mri.NeedWastAge
                            FROM #MaterialRegStatusItem mri
                                inner join dbo.M2_OutOrderlists ool on ool.Id=mri.FromId
	                            inner join dbo.M2_OutOrderlists_wl l on mri.FromListId=l.Id
	                            INNER JOIN dbo.M2_OutOrder oo ON l.outID = oo.ID
	                            left JOIN dbo.product p ON l.ProductID = p.ord
	                            LEFT JOIN dbo.product zp ON ool.ProductID= zp.ord
                                INNER join dbo.KuIn ki on mri.KuinID=ki.Ord
                            where mri.FromType=2";
                        if (Batch)
                        {
                            mainsql = @"select * from (SELECT cast(mri.KuinID as varchar(20)) + 'two' iden
                                            ,0 bid,l.ID ListID,0 ID,l.ID newbid
                                            ,oo.title billtitle
                                            ,ki.title as KuInTitle
                                            ,l.ProductID ProductID
                                            ,zp.title protitle
                                            ,(case ISNULL(zp.del, 99) when 1 then '' when 2 then '(已删除)' when 99 then '产品已彻底删除' end) deltitle
                                            ,zp.order1
                                            ,zp.type1
                                            ,l.unit1 as unit,l.BomListID BomList
                                            ,isnull(mri.KuinNum, 0) sx
                                            ,isnull(mri.KuinRegCompletedNum, 0) cbc
                                            ,isnull(mri.KuinWaitRegNum, 0) num1
                                            ,isnull(mri.KuinNum,0) ky
                                            ,(CASE oo.[Status] WHEN 1 THEN '正常' WHEN 2 THEN '终止' END) [Status]
                                            ,1 bl
                                            ,1 zy
                                            ,0 mapnum,'' zdmapnum
                                            ,NULL mapping
                                            ,(case isnull(oo.wwType,0) when 0 then 3 when 1 then 4 end) poTypeV
                                            , oo.gys
                                            ,mri.KuinID as ForKuinID
                                            ,1 as canRk
                                            ,ki.Date5 as date1
                                            ,(select top 1 e.sort1 from dbo.sortonehy e where e.gate2 = 61 and e.ord=l.unit1) unname
                                            ,l.remark intro
                                            ,0 as NeedWastAge
                                    FROM (select distinct KuinID,FromId,KuinRegCompletedNum,KuinWaitRegNum,FromType,KuinNum from #MaterialRegStatusItem) mri
                                        INNER join dbo.KuIn ki on mri.KuinID=ki.Ord
                                        inner join dbo.M2_OutOrderlists l on l.Id=mri.FromId
                                        INNER JOIN dbo.M2_OutOrder oo ON l.outID = oo.ID
                                        LEFT JOIN dbo.product zp ON l.ProductID = zp.ord
                                    where mri.FromType=2
                                    union all
                                    SELECT cast(mri.KuinID as varchar(20)) + 'two' iden
                                        ,ool.ID bid,l.ID ListID,0 ID,ool.ID newbid
                                        ,oo.title billtitle
                                        ,ki.title as KuInTitle
                                        ,l.ProductID ProductID,p.title protitle
                                        ,(case ISNULL(p.del,99) when 1 then '' when 2 then '(已删除)' when 99 then '产品已彻底删除' end) deltitle
                                        ,p.order1
                                        ,p.type1
                                        ,l.unit,ool.BomListID BomList
                                        ,isnull(mri.NeedNum, 0) sx
                                        ,isnull(mri.RegedNum, 0) cbc
                                        ,isnull(mri.WaitRegNum, 0) num1
                                        ,isnull(mri.EnableRoomKuNum,0) ky
                                        ,(CASE oo.[Status] WHEN 1 THEN '正常' WHEN 2 THEN '终止' END) [Status]
                                        ,ISNULL(mri.NeedBL,1) bl
                                        ,1 zy
                                        ,0 mapnum,'' zdmapnum,NULL mapping
                                        ,(case isnull(oo.wwType,0) when 0 then 3 when 1 then 4 end) poTypeV
                                        ,oo.gys
                                        ,mri.KuinID as ForKuinID
                                        ,1 as canRk
                                        ,ki.Date5 as date1
                                        ,(select top 1 e.sort1 from dbo.sortonehy e where e.gate2 = 61 and e.ord=l.unit) unname
                                        ,l.remark intro
                                        ,mri.NeedWastAge
                                    FROM #MaterialRegStatusItem mri
                                        INNER join dbo.KuIn ki on mri.KuinID=ki.Ord
                                        inner join dbo.M2_OutOrderlists ool on ool.Id=mri.FromId
                                        inner join dbo.M2_OutOrderlists_wl l on mri.FromListId=l.Id
                                        INNER JOIN dbo.M2_OutOrder oo ON l.outID = oo.ID
                                        left JOIN dbo.product p ON l.ProductID = p.ord
                                    where mri.FromType=2) tblist order by tblist.date1,tblist.ForKuinID,tblist.bid asc";

                        }
                        break;
                    case 2:
                        mainsql = @"SELECT mrl.ID,isnull(mrl.ListID,0) ListID,p.title protitle,(case ISNULL(p.del,99) when 1 then '' when 2 then '(已删除)' when 99 then '产品已彻底删除' end) deltitle,p.order1,p.type1,mrl.unit
                                            ,isnull(mri.NeedNum,0) sx
                                            ,isnull(mri.RegedNum,0) cbc
                                            ,isnull(mrl.Num1,0) num1
                                            ,0 cbprice
                                            ,0 cbmoney
                                            ,isnull(mri.EnableRoomKuNum,0) ky
                                            ,mrl.zy zy,mrl.intro,
                                            '' ph, '' xlh , '' datesc,'' dateyx,mrl.ProductID,mrl.BomList
                                            ,(select sum(mro.num) from dbo.M2_RegisterOccupy mro where mrl.ID = mro.MRLID AND mro.isOld = 0)mapnum,'' zdmapnum,mapping.mapping
                                            ,ISNULL(mri.NeedBL,1) bl
                                            ,mrl.poTypeV,oo.gys,isnull(wa.Id,oo.ID) AS bid
                                            ,(select top 1 e.sort1 from dbo.sortonehy e where e.gate2 = 61 and e.ord=mrl.unit) unname
                                            ,mr.ForKuinID
                                            ,(CASE WHEN pdetail.qx_open = 1 THEN 1 ELSE 0 END) pdetail
                                            ,isnull(mri.NeedWastAge,0) as NeedWastAge
                                        FROM dbo.M2_MaterialRegisterLists mrl
                                        INNER JOIN dbo.M2_MaterialRegisters mr ON mrl.MRID = mr.ID
                                        LEFT JOIN dbo.product p ON mrl.ProductID = p.ord
                                        LEFT JOIN dbo.M2_WorkAssigns wa ON mr.bid = wa.ID and mr.poType in(1,2)
                                        LEFT JOIN dbo.M2_WorkAssignLists wal ON wa.ID = wal.WAID AND mrl.ListID = wal.ID
                                        LEFT JOIN dbo.M2_OutOrderlists ool ON mr.bid = ool.ID
                                        LEFT JOIN dbo.M2_OutOrder oo ON ool.outID = oo.ID and mr.poType in(3,4)
                                        LEFT JOIN dbo.M2_OutOrderlists_wl l ON ool.outID = l.outID AND mrl.ListID = l.ID AND (ISNULL(ool.molist,0) = ISNULL(l.pmolist,1) OR l.pmolist IS NULL)
                                        LEFT JOIN dbo.M2_BOMList bl ON l.BomListID = bl.ID
                                        LEFT JOIN dbo.power pdetail ON pdetail.ord = " + UserInfo.UserID + @" AND pdetail.sort1 = 21 AND pdetail.sort2 = 14
                                        LEFT JOIN (
	                                        SELECT
                                            MRLID,
                                            mapping = (STUFF((SELECT '|' + CAST(kuoutlist2 AS VARCHAR(20))+','+ CAST(num AS VARCHAR(20))
                                                             FROM M2_RegisterOccupy
                                                             WHERE MRLID = A.MRLID
                                                             FOR xml path('')),1,1,''))
	                                        FROM M2_RegisterOccupy A WHERE A.MRLID IN(SELECT ID FROM dbo.M2_MaterialRegisterLists WHERE MRID = " + Bill.Ord + @")
	                                        GROUP by A.MRLID
                                        ) mapping ON mrl.ID = mapping.MRLID
                                        left join #MaterialRegStatusItem mri on mri.KuinID=mr.ForKuinID and mri.ProductID=mrl.ProductID and mri.FromListId=mrl.ListID
                                        where mrl.MRID =" + Bill.Ord + @" and isnull(mrl.Mergeinx,0) <= 0
                                        order by mrl.ID asc";
                        break;
                }
                mainsql += "; drop table #MaterialRegStatusItem";
                tb = Sql.GetTable(mainsql);
                if (notOpenPublicConn) { Sql.ClosePublicConnection(); }
            }
            else
            {
                mainsql = @"SELECT top 0  null iden,null bid,null ListID,null newbid
                                ,null billID
                                ,null billtitle
                                ,null KuInTitle
                                ,null ProductID,null protitle
                                ,null deltitle
	                            ,null order1
	                            ,null type1
	                            ,null unit,null BomList
                                ,null sx
	                            ,null cbc
	                            ,null num1,null ky
	                            ,null BH
                                ,null [Status]
	                            ,null ztitle
                                ,null zorder1, null bl
                                ,null ID,NULL cbprice, NULL cbmoney,1 zy
	                            ,NULL ph, NULL xlh,NULL datesc, NULL dateyx,0 mapnum,'' zdmapnum
	                            ,NULL mapping,null poTypeV,null gys
                                ,null PGNum
                                ,null ProductModel
                                ,null KuInNum
                                ,null KuinRegCompletedNum
                                ,null totalnum
                                ,null ForKuinID
                                ,1 as canRk
                                ,null as date1
                                ,null unname
                                ,null as zProductID,null intro,null NeedWastAge";
                tb = Sql.GetTable(mainsql);
            }

            return tb;
        }
        #endregion

        #region 左侧导航处理过程
        //左侧导航加载事件
        protected override void LeftPage_Load(sdk.leftmenuframe.LeftMenuFrameClass LeftPage, sdk.leftmenuframe.LeftMenuFrameInitEventType initType)
        {
            if (Bill.UiState != BillViewStateEnum.Add)
            {
                base.LeftPage_Load(LeftPage, initType);
                return;
            }
            LeftPage.MainTitle = "选择登记单据";

            var existsfirst = false;
            if (existsWorkAssginReg())
            {
                LeftPage.Title = "生产派工";
                LeftPage.TreeTitle = "生产派工";
                LeftPage.ExistsSearch = true;
                LeftPage.ExistsDaysSearch = true;
                LeftPage.Tree.OnlyLeafNodeChoose = true;
                LeftPage.SearchItems.Add("订单主题", "ManuOrderTitle");
                LeftPage.SearchItems.Add("派工主题", "WorkAssignsTitle");
                LeftPage.SearchItems.Add("派工编号", "WorkAssignsBH");
                LeftPage.SearchItems.Add("入库主题", "KuInTitle");
                LeftPage.SearchItems.Add("产品名称", "ProductName");
                LeftPage.SearchItems.Add("产品编号", "ProductOrder");
                LeftPage.SearchItems.Add("产品型号", "ProductType");
                LeftPage.Tree.CheckModel = sdk.treeview.TreeCheckBoxModel.Radio;
                LeftPage.TreeDataInterface = materialRegHelper.LeftTreeSourceForWorkAssigns;
                existsfirst = true;
            }

            if (SystemPowerClass.ExistsModule(18700))
            {
                LeftMenuFrameClass ww = existsfirst ? (new LeftMenuFrameClass(LeftFrameTypeEnum.TreeSearchPage)) : LeftPage;
                ww.Title = "整单委外";
                ww.TreeTitle = "整单委外";
                ww.ExistsSearch = true;
                ww.ExistsDaysSearch = true;
                ww.Tree.OnlyLeafNodeChoose = true;
                ww.SearchItems.Add("订单主题", "ManuOrderTitle");
                ww.SearchItems.Add("委外主题", "WWOutTitle");
                ww.SearchItems.Add("委外编号", "WWOutBH");
                ww.SearchItems.Add("入库主题", "KuInTitle");
                ww.SearchItems.Add("产品名称", "ProductName");
                ww.SearchItems.Add("产品编号", "ProductOrder");
                ww.SearchItems.Add("产品型号", "ProductType");
                ww.Tree.CheckModel = sdk.treeview.TreeCheckBoxModel.Radio;
                ww.TreeDataInterface = materialRegHelper.LeftTreeSourceForWWOut;
                if (existsfirst) { LeftPage.Outers.Add(ww); }
            }


            int fromType = GetInt("fromType");
            int fromId = TextCoderClass.DeUrl(GetText("fromID"));
            LeftPage.TabSelectIndex = fromType > 0 ? fromType - 1 : fromType;
            LeftPage.LoadData();
            if (fromId > 0)
            {

                var nodes = LeftPage.TabSelectIndex == 0 ? LeftPage.Tree.Nodes : LeftPage.Outers[LeftPage.TabSelectIndex - 1].Tree.Nodes;
                foreach (var item in nodes)
                {
                    if (item.ID == fromId.ToString() || fromType == 2)
                    {
                        item.Checked = true;

                        if (item.Nodes != null && item.Nodes.Count > 0 || fromType == 2)
                        {

                            var child = item.Nodes.FirstOrDefault();
                            child.Checked = true;
                            FromKuinId = child.ID.ToInt(0);
                            if (fromType == 2)
                            {
                                if (child.Nodes != null && child.Nodes.Count > 0)
                                {
                                    var childchild = child.Nodes.FirstOrDefault();
                                    childchild.Checked = true;
                                    FromKuinId = childchild.ID.ToInt(0);
                                }
                            }
                        }
                    }
                }
            }
        }
        //左侧导航点击回调事件
        protected override void App_OnLeftPageTreeClick(BillCallBackerClass callbacker, LeftPageTreeClickEventClass eventdatas, BillSaveDataCollection SaveDatas)
        {
            //获取新增的行 (没有派工，则点击过来的只能是第2个选项卡  0=派工 1=委外)
            int tabIndex = existsWorkAssginReg() ? eventdatas.SSTabIndex : 1;
            callbacker.ListView.ClearRows("rglvw");
            var existsdata = false;
            foreach (var node in eventdatas.NewAddNodes)
            {
                #region 遍历左侧导航节点
                if (Convert.ToInt32(node.ID) > 0)
                {
                    int[] listId = new int[] { node.ID.ToInt() };
                    DataTable adddt = GetLvwDataSqlTable(listId, tabIndex);
                    if (adddt != null && adddt.Rows.Count > 0)
                    {
                        FromKuinId = ConvertHelper.ToInt(adddt.Rows[0]["ForKuinID"].ToString());
                        int poTypeV = ConvertHelper.ToInt(adddt.Rows[0]["poTypeV"].ToString());
                        int poType = 1;
                        if (poTypeV == 3 || poTypeV == 4)
                        {
                            poType = 3;
                        }
                        callbacker.UpdateFieldValue("bid", adddt.Rows[0]["bid"].ToString());
						callbacker.UpdateFieldValue("poType", poType.ToString());
						callbacker.UpdateFieldValue("poTypeV", adddt.Rows[0]["poTypeV"].ToString());
                        callbacker.UpdateFieldValue("billID", adddt.Rows[0]["billID"].ToString());
                        callbacker.UpdateFieldValue("billname", adddt.Rows[0]["billtitle"].ToString());
                        callbacker.UpdateFieldValue("billBH", adddt.Rows[0]["BH"].ToString());
                        callbacker.UpdateFieldValue("billstatus", adddt.Rows[0]["Status"].ToString());
                        callbacker.UpdateFieldValue("proname", adddt.Rows[0]["ztitle"].ToString());
                        callbacker.UpdateFieldValue("proorder", adddt.Rows[0]["zorder1"].ToString());
                        callbacker.UpdateFieldValue("ky", adddt.Rows[0]["ky"].ToString());
                        adddt.Columns["ky"].ReadOnly = false;
                        callbacker.UpdateFieldValue("PGNum", adddt.Rows[0]["PGNum"].ToString());
                        callbacker.UpdateFieldValue("title", adddt.Rows[0]["billtitle"].ToString());
                        callbacker.UpdateFieldValue("KuInNum", adddt.Rows[0]["KuInNum"].ToString());
                        callbacker.UpdateFieldValue("KuinRegCompletedNum", adddt.Rows[0]["KuinRegCompletedNum"].ToString());
                        callbacker.UpdateFieldValue("totalnum", adddt.Rows[0]["totalnum"].ToString());
                        callbacker.UpdateFieldValue("ProductModel", adddt.Rows[0]["ProductModel"].ToString());
                        callbacker.UpdateFieldValue("ForKuinID", FromKuinId.ToString());
                        callbacker.UpdateFieldValue("canRk", adddt.Rows[0]["canRk"].ToString());
                        callbacker.UpdateFieldValue("date1", adddt.Rows[0]["date1"].ToString());
                        callbacker.UpdateFieldValue("KuInTitle", adddt.Rows[0]["KuInTitle"].ToString());
                        callbacker.UpdateFieldValue("unname", adddt.Rows[0]["unname"].ToString());

                        int ProductId = ConvertHelper.ToInt(adddt.Rows[0]["zProductID"].ToString());
                        callbacker.UpdateFieldValue("ProductID", adddt.Rows[0]["zProductID"].ToString());
                        BillInfoClass Bic = BillCommModule.GetBill(16001, ProductId, adddt.Rows[0]["ztitle"].ToString());
                        BillHtmlField proname = callbacker.NewBill.Groups[0].Fields.Add<BillHtmlField>("产品名称", "proname");
                        proname.FormatHTML = Bic.linktitle.IsNull(adddt.Rows[0]["ztitle"].ToString());
                        proname.CDisplay(BFDisplayEnum.ReadOnly);
                        callbacker.UpdateField("proname");

                        BillHtmlField billname = callbacker.NewBill.Groups[0].Fields.Add<BillHtmlField>("关联单据", "billname").CDisplay(BFDisplayEnum.ReadOnly);
                        if (poType == 1 || poType == 3)
                        {
                            int billID = ConvertHelper.ToInt(adddt.Rows[0]["billID"].ToString());
                            billname.FormatHTML = adddt.Rows[0]["billtitle"].ToString() + "<font color=\"red\">（" + this.GetMaterialRegStatusName(poType, billID) + "）</font>";
                        }
                        BillHtmlField KuInNum = callbacker.NewBill.Groups[0].Fields.Add<BillHtmlField>("入库数量", "KuInNumInfo");
                        string htmlinfo = string.Format("{0:N" + SystemInfoClass.NumberBit + "}", ConvertHelper.ToDecimal(adddt.Rows[0]["KuInNum"])).ToString();
                        if (!string.IsNullOrWhiteSpace(adddt.Rows[0]["KuInTitle"].ToString()))
                        {

                            int ForKuinID = ConvertHelper.ToInt(adddt.Rows[0]["ForKuinID"].ToString());
                            if (ExistsRangePower(SQLPowerTypeEnum.入库权限, 14, UserInfo.UserID))
                            {
                                htmlinfo += "<a href=\"" + this.VirPath + "SYSA/store/contentrk.asp?ord=" + TextCoderClass.PwUrl(ForKuinID) + "&view=details\" target=\"view_window\" style=\"text-decoration:none;\"><font color=\"red\">（" + adddt.Rows[0]["KuInTitle"].ToString() + "）</font></a>";
                            }
                            else
                            {
                                htmlinfo += "<font color=\"red\">（" + adddt.Rows[0]["KuInTitle"].ToString() + "）</font>";
                            }

                        }
                        KuInNum.FormatHTML = htmlinfo;
                        callbacker.UpdateField("KuInNumInfo");

                        string PGNumName = "委外";
                        if (tabIndex == 0)
                        {
                            PGNumName = "派工";
                        }
                        BillNumberBoxField pGNum = callbacker.NewBill.Groups[0].Fields.AddNumber(PGNumName + "数量", "PGNum").CDisplay(BFDisplayEnum.ReadOnly);
                        callbacker.UpdateField("PGNum");


                        List<BillCustomInheritType> InheritTypes = new List<BillCustomInheritType>();
                        BillCustomInheritType CIT = new BillCustomInheritType();
                        CIT.InheritIdBillType = BillApplyTypeEnum.B54002_生产派工单;
                        CIT.InheritIdListType = 2;
                        CIT.InheritFieldKey = "listID";
                        CIT.RowFilterCode = "poTypeV=1";
                        InheritTypes.Add(CIT);
                        CIT = new BillCustomInheritType();
                        CIT.InheritIdBillType = BillApplyTypeEnum.B54005_生产返工单;
                        CIT.InheritIdListType = 1;
                        CIT.InheritFieldKey = "listID";
                        CIT.RowFilterCode = "poTypeV=2";
                        InheritTypes.Add(CIT);
                        //执行listview中新增行操作
                        switch (poType)
                        {
                            case 1: adddt = BillCustomFieldsHelper.LoadListViewInheritIdsData(adddt, BillApplyTypeEnum.B55004_生产物料登记, 1, InheritTypes, 0, true); break;
                            case 2: adddt = BillCustomFieldsHelper.LoadListViewInheritIdsData(adddt, BillApplyTypeEnum.B55004_生产物料登记, 1, InheritTypes, 0, true); break;
                            case 3: adddt = BillCustomFieldsHelper.LoadListViewInheritIdData(adddt, BillApplyTypeEnum.B55004_生产物料登记, 1, BillApplyTypeEnum.B54003_整单委外, 2, "ListID", 0); break;
                            case 4: adddt = BillCustomFieldsHelper.LoadListViewInheritIdData(adddt, BillApplyTypeEnum.B55004_生产物料登记, 1, BillApplyTypeEnum.B54006_生产工序委外单, 2, "ListID", 0); break;
                        }
                        existsdata = true;
                        callbacker.ListView.AddRows("rglvw", adddt);
                    }
                }
                #endregion
            }
            if (existsdata == false)
            {
                callbacker.UpdateFieldValue("title", "");
                callbacker.UpdateFieldValue("billname", "");
                callbacker.UpdateFieldValue("billBH", "");
                callbacker.UpdateFieldValue("billstatus", "");
                callbacker.UpdateFieldValue("proname", "");
                callbacker.UpdateFieldValue("proorder", "");
                callbacker.UpdateFieldValue("PGNum", "0");
                callbacker.UpdateFieldValue("ProductModel", "");
                callbacker.UpdateFieldValue("KuinRegCompletedNum", "0");
                var htmlf1 = callbacker.NewBill.BaseCroup.Fields.AddHtml("关联单据", "billname").CColSpan(1);
                var htmlf2 = callbacker.NewBill.BaseCroup.Fields.AddHtml("产品名称", "proname").CColSpan(1);
                var htmlf3 = callbacker.NewBill.BaseCroup.Fields.AddHtml("入库数量", "KuInNumInfo").CColSpan(1);
                callbacker.UpdateField("proname");
                callbacker.UpdateField("billname");
                callbacker.UpdateField("KuInNumInfo");
                callbacker.ListView.ClearRows("rglvw", true);
                callbacker.UpdateFieldValue("totalnum", "0");
                callbacker.UpdateFieldValue("ForKuinID", "0");
            }
        }
        #endregion

        #region 界面UI呈现、回调过程

        /// <summary>单据初始化过程 </summary>
        public override void OnBillInit(BillInitEventType callType)
        {
            CssTexts.Append(".f_htmlfield{margin-left: 3px !important;}");
            #region 单据基本信息
            string productName = " p.title ";
            if (Bill.UiState == BillViewStateEnum.Details)
            {

                productName = "(CASE ISNULL(p.del,99) WHEN 1 THEN p.title WHEN 2 THEN p.title+'<font color=red>(已删除)</font>' WHEN 99 THEN '<font color=red>产品已彻底删除</font>' END)";
            }
            Bill.UI.Title = "物料登记" + Bill.UiStateText;
            Bill.Source.MainSql = @"SELECT mr.title,mr.MOBH,mr.date1,mr.intro,ISNULL(wa.title,oo.title) billname,ISNULL(wa.ID,oo.ID) billID,mr.poType,mr.bid,ISNULL(wa.Creator,oo.creator) billCate,
                                ISNULL(wa.WABH,oo.sn) billBH,(CASE WHEN ISNULL(wa.Status,-1) = -1 THEN (CASE ISNULL(oo.Stopstatus,0) WHEN 0 THEN '正常' WHEN 1 THEN '终止' END)
                                 ELSE (CASE ISNULL(wa.[Status],1) WHEN 1 THEN '正常' WHEN 2 THEN '终止' END) END) billstatus,
                                " + productName + @" as proname,p.order1 proorder,mr.indate,mr.del,mr.creator,isnull(mr.status,-1) status,cate.name cate,
                                (CASE WHEN ISNULL(cc.id,0) > 0 THEN '已核算' ELSE '未核算' END) hszt,isnull(mr.totalnum,0) as totalnum,mr.OrderType,p.type1 as ProductModel
                                ,isnull(wa.NumMake,ool.num1) as PGNum
                                ,mr.ForKuinID
                                ,p.ord as ProductID
                                ,1 as delId
                                FROM dbo.M2_MaterialRegisters mr 
                                LEFT JOIN M2_WorkAssigns wa ON mr.OrderType = 1 AND mr.bid = wa.ID AND mr.poType <= 2
                                LEFT JOIN dbo.M2_OutOrderlists ool ON mr.OrderType = 1 AND mr.bid = ool.ID AND mr.poType > 2
                                LEFT JOIN dbo.M2_OutOrder oo ON ool.outID = oo.ID
                                LEFT JOIN dbo.product p ON ISNULL(wa.ProductID,ool.productid) = p.ord
                                LEFT JOIN dbo.gate cate ON mr.Creator = cate.ord
                                LEFT JOIN dbo.M2_CostComputation cc ON cc.complete1 = 1 AND DATEDIFF(MONTH,cc.date1,mr.date1) = 0 
                                WHERE mr.ID = @ord";
            bool isCostAnalysis = CostAnalysisModule.IsCostAnalysis(BillApplyTypeEnum.B55004_生产物料登记, Bill.Ord, "");
            if (Bill.UiState == BillViewStateEnum.Details && Bill.Source.Items["del"].ToString().ToInt() == 1)
            {
                int status = Bill.Source.Items["status"].ToString().ToInt();
                if (Bill.Source.Items["creator"].ToString().ToInt() == UserInfo.UserID && ((status == 2 && string.IsNullOrWhiteSpace(Bill.ApproveInfo.GetApprovers(Bill.Ord, true).Trim())) || status == 1 || status == 0))
                {
                    RemindClass.cancelRemind(55004, Bill.Ord.ToString());
                }
                Bill.CommandButtons.Add(BillCommCmdButtonEnum.ApproveButton);

                if (Bill.ApproveInfo.ApproveStatusValue(Bill.Ord) != Bill.ApproveInfo.ApproveStatus.Reject && !isCostAnalysis)
                {
                    Bill.CommandButtons.Add(BillCommCmdButtonEnum.UpdateApproveButton);
                }
                Bill.CommandButtons.Add(BillCommCmdButtonEnum.ModifyApproveButton);

                if (!isCostAnalysis)
                {
                    if (Bill.ApproveInfo.ApproveStatusValue(Bill.Ord) == Bill.ApproveInfo.ApproveStatus.NoSp
                        || Bill.ApproveInfo.ApproveStatusValue(Bill.Ord) == Bill.ApproveInfo.ApproveStatus.Wait)
                    {
                        Bill.CommandButtons.Add(BillCommCmdButtonEnum.UpdateButton);
                    }
                    Bill.CommandButtons.Add(BillCommCmdButtonEnum.DelButton);
                }

                Bill.CommandButtons.Add(BillCommCmdButtonEnum.PrintButton);
            }
            Bill.CommandButtons.Add(BillCommCmdButtonEnum.SaveButton);
            if (Bill.UiState == BillViewStateEnum.Add || Bill.UiState == BillViewStateEnum.TempSave)
            {
                Bill.CommandButtons.Add(BillCommCmdButtonEnum.IncreaseButton);
                Bill.CommandButtons.Add(BillCommCmdButtonEnum.TemporaryButton);
            }
            Bill.CommandButtons.Add(BillCommCmdButtonEnum.ResetButton);

            BillFieldGroupClass basegp = Bill.BaseCroup;
            string deftitle = string.Empty;
            int tabindex = 0;
            DataTable dtl = null;
            if (GetInt("fromType") > 0 && string.IsNullOrWhiteSpace(GetText("fromID")) == false)
            {
                switch (GetInt("fromType"))
                {
                    case 1:
                        tabindex = 0;
                        deftitle = Sql.GetValue("SELECT title FROM dbo.M2_WorkAssigns WHERE ID = " + TextCoderClass.DeUrl(GetText("fromID"))).IsNull("");
                        break;
                    case 2:
                        tabindex = 1;
                        deftitle = Sql.GetValue(@"select title from M2_OutOrder oo 
                                                    inner join M2_OutOrderlists ool on oo.ID = ool.outID
                                                    and ool.ID =" + TextCoderClass.DeUrl(GetText("fromID"))).IsNull("");

                        break;
                }
                basegp.Fields.AddText("登记主题", "title", deftitle).CNotNull().CMax(100);

                if (FromKuinId > 0)
                {
                    dtl = GetLvwDataSqlTable(new int[] { FromKuinId }, tabindex);
                }
            }
            else
            {
                basegp.Fields.AddAutoSN("登记主题", "title").CNotNull().CMax(100);
            }

            if (Bill.UiState == BillViewStateEnum.TempSave)
            {
                FromKuinId = ConvertHelper.ToInt(Bill.Source.Items["ForKuinID"].ToString());
                if (FromKuinId > 0)
                {
                    switch (GetInt("fromType"))
                    {
                        case 1:
                            tabindex = 0;
                            break;
                        case 2:
                            tabindex = 1;
                            break;
                    }
                    dtl = GetLvwDataSqlTable(new int[] { FromKuinId }, tabindex);
                }
            }
            int poType = 1;
            if (tabindex == 1) {
                poType = 3;
            }

            basegp.Fields.AddAutoSN("登记编号", "MOBH").CNotNull().CMax(50);
            var date1 = basegp.Fields.AddDate("登记日期", "date1", DateTime.Now.ToString()).CNotNull();
            if (Bill.UiState == BillViewStateEnum.Details)
            {
                basegp.Fields.AddText("核算状态", "hszt").Visible = SystemPowerClass.ExistsModule(18500);
                basegp.Fields.AddText("添加人员", "cate");
                basegp.Fields.AddText("添加时间", "indate");
                basegp.Fields.AddText("审批状态", "spstate").Value = Bill.ApproveInfo.GetApproveStatus(Bill.Ord, false);
                basegp.Fields.AddText("当前审批人", "currsp").Value = Bill.ApproveInfo.GetApprovers(Bill.Ord, true);
                basegp.Fields.AddText("最后审批人", "lastsp").Value = Bill.ApproveInfo.GetLastApprovers(Bill.Ord, true);
            }
            else
            {
                BillHtmlField billname = basegp.Fields.Add<BillHtmlField>("关联单据", "billname");
                billname.CDisplay(BFDisplayEnum.ReadOnly);
                string pgname = "派工";
                if (Bill.UiState != BillViewStateEnum.Add && Bill.UiState != BillViewStateEnum.TempSave)
                {
                    poType = Bill.Source.Items["poType"].ToString().ToInt();
                    int billID = Bill.Source.Items["billID"].ToString().ToInt();
                    billname.FormatHTML = Bill.Source.Items["billname"].ToString();
                    if (poType == 1 || poType == 3)
                    {
                        billname.FormatHTML = Bill.Source.Items["billname"].ToString() + "<font color=\"red\">（" + this.GetMaterialRegStatusName(poType, billID) + "）</font>";
                    }

                    if (poType == 3 || poType == 4)
                    {
                        pgname = "委外";
                    }

                }
                else
                {
                    if (tabindex == 1)
                    {
                        pgname = "委外";
                    }
                }
                BillTextBoxField billBH = basegp.Fields.AddText("单据编号", "billBH").CDisplay(BFDisplayEnum.ReadOnly);
                BillNumberBoxField pGNum = basegp.Fields.AddNumber(pgname + "数量", "PGNum").CDisplay(BFDisplayEnum.ReadOnly);

                if (dtl != null && dtl.Rows.Count > 0)
                {
                    int poTypeV = ConvertHelper.ToInt(dtl.Rows[0]["poTypeV"].ToString());
                    if (poTypeV == 3 || poTypeV == 4) {
                        poType = 3;
                    }
                    int billID = ConvertHelper.ToInt(dtl.Rows[0]["billID"].ToString());
                    billname.FormatHTML = dtl.Rows[0]["billtitle"].ToString() + "<font color=\"red\">（" + this.GetMaterialRegStatusName(poType, billID) + "）</font>";
                    billBH.DefValue = dtl.Rows[0]["BH"].ToString();
                    pGNum.DefValue = dtl.Rows[0]["PGNum"].ToString();
                }
            }

            BillHtmlField proname = basegp.Fields.Add<BillHtmlField>("产品名称", "proname");
            if (Bill.Ord > 0)
            {
                int ProductId = ConvertHelper.ToInt(Bill.Source.Items["ProductID"]);
                BillInfoClass Bic = BillCommModule.GetBill(16001, ProductId, Bill.Source.Items["proname"].ToString());
                proname.FormatHTML = Bic.linktitle.IsNull(Bill.Source.Items["proname"].ToString());
                proname.CDisplay(BFDisplayEnum.ReadOnly);
            }
            BillTextBoxField proorder = basegp.Fields.AddText("产品编号", "proorder").CDisplay(BFDisplayEnum.ReadOnly);
            BillTextBoxField productModel = basegp.Fields.AddText("产品型号", "ProductModel").CDisplay(BFDisplayEnum.ReadOnly);

            if (Bill.UiState == BillViewStateEnum.Details)
            {
                #region 详情

                int billID = Bill.Source.Items["billID"].ToString().ToInt(); //单据ID
                BillApplyTypeEnum powem = BillApplyTypeEnum.B54002_生产派工单;
                switch (Bill.Source.Items["poType"].ToString().ToInt())
                {
                    //1 派工  2 返工 3 整单委外 4 工序委外
                    case 1:
                        powem = BillApplyTypeEnum.B54002_生产派工单;
                        break;
                    case 2:
                        powem = BillApplyTypeEnum.B54005_生产返工单;
                        break;
                    case 3:
                        powem = BillApplyTypeEnum.B54003_整单委外;
                        break;
                    case 4:
                        powem = BillApplyTypeEnum.B54006_生产工序委外单;
                        break;
                }

                BillInfoClass Bic = BillCommModule.GetBill((int)powem, billID, Bill.Source.Items["billname"].ToString());
                BillHtmlField billname = basegp.Fields.Add<BillHtmlField>("关联单据", "billname");
                billname.FormatHTML = Bic.linktitle.IsNull(Bill.Source.Items["billname"].ToString());
                billname.CDisplay(BFDisplayEnum.ReadOnly);

                BillLinkBoxField ForKuinID = basegp.Fields.Add<BillLinkBoxField>("关联入库", "ForKuinID");
                ForKuinID.LinkConvertProc = delegate (string id, out string title, out string url)
                {
                    var rkinfo = Sql.GetStructs("select top 1 ki.title, ki.cateid  from dbo.KuIn ki where ki.ord=" + id, new
                    {
                        title = "",
                        cateid = 0
                    }).FirstOrDefault();
                    if (rkinfo == null || string.IsNullOrWhiteSpace(rkinfo.title))
                    {
                        title = "<span style=\"color:red\">未关联入库</span>";
                        url = "";
                    }
                    else
                    {
                        url = ExistsRangePower(SQLPowerTypeEnum.入库权限, 14, rkinfo.cateid)
                        ? (this.VirPath + "SYSA/store/contentrk.asp?ord=" + TextCoderClass.PwUrl(ConvertHelper.ToInt(id)) + "&view =details")
                        : "";
                        title = rkinfo.title;
                    }
                };

                BillNumberBoxField regedNum = basegp.Fields.AddNumber("登记数量", "totalnum").CDisplay(BFDisplayEnum.ReadOnly);
                basegp.Fields.AddEditor("概要", "intro");
                basegp.Fields.AddBarCode("条码", "txm").CGenerateLabel(true).CColSpan(3).CBillSNValue();
                #endregion
            }
            else
            {
                var KuInNumValue = basegp.Fields.AddText("", "KuInNum").CDisplay(BFDisplayEnum.Hidden);
                if (Bill.UiState == BillViewStateEnum.Add || Bill.UiState == BillViewStateEnum.TempSave)
                {
                    #region 添加
                    var ForKuinID = basegp.Fields.AddText("", "ForKuinID").CDisplay(BFDisplayEnum.Hidden);


                    BillHtmlField KuInNumInfo = basegp.Fields.Add<BillHtmlField>("入库数量", "KuInNumInfo");
                    KuInNumInfo.CDisplay(BFDisplayEnum.ReadOnly);
                    var KuInTitle = basegp.Fields.AddText("", "KuInTitle").CDisplay(BFDisplayEnum.Hidden);
                    BillNumberBoxField regedNum = basegp.Fields.AddNumber("已登记数量", "KuinRegCompletedNum").CDisplay(BFDisplayEnum.ReadOnly);

                    if (dtl != null && dtl.Rows.Count > 0)
                    {
                        ForKuinID.DefValue = dtl.Rows[0]["ForKuinID"].ToString();
                        ForKuinID.Value = ForKuinID.DefValue;
                        KuInTitle.DefValue = dtl.Rows[0]["KuInTitle"].ToString();
                        KuInTitle.Value = KuInTitle.DefValue;
                        regedNum.DefValue = dtl.Rows[0]["KuinRegCompletedNum"].ToString();
                        regedNum.Value = regedNum.DefValue;
                        KuInNumValue.DefValue = dtl.Rows[0]["KuInNum"].ToString();
                        KuInNumValue.Value = KuInNumValue.DefValue;
                        string htmlinfo = string.Format("{0:N" + SystemInfoClass.NumberBit + "}", ConvertHelper.ToDecimal(dtl.Rows[0]["KuInNum"])).ToString();
                        if (!string.IsNullOrWhiteSpace(dtl.Rows[0]["KuInTitle"].ToString()))
                        {
                            if (ExistsRangePower(SQLPowerTypeEnum.入库权限, 14, UserInfo.UserID))
                            {
                                htmlinfo += "<a href=\"" + this.VirPath + "SYSA/store/contentrk.asp?ord=" + TextCoderClass.PwUrl(ConvertHelper.ToInt(dtl.Rows[0]["ForKuinID"].ToString())) + "&view=details\" target=\"view_window\" style=\"text-decoration:none;\"><font color=\"red\">（" + dtl.Rows[0]["KuInTitle"].ToString() + "）</font></a>";
                            }
                            else
                            {
                                htmlinfo += "<font color=\"red\">（" + dtl.Rows[0]["KuInTitle"].ToString() + "）</font>";
                            }

                        }
                        KuInNumInfo.FormatHTML = htmlinfo;
                    }
                    #endregion
                }
                else
                {
                    #region 修改
                    poType = Bill.Source.Items["poType"].ToString().ToInt();
                    List<int> ids = new List<int>() { Bill.Source.Items["billID"].ToString().ToInt() };  //获取派工或委外单id
                    if (poType == 3 || poType == 4)
                    {
                        poType = 2;
                    }
                    else if (poType == 1 || poType == 2)
                    {
                        poType = 1;
                    }
                    List<int> kuInIds = MaterialRegHelper.Instance.GetForKuInIds(ids, poType);
                    int mykuin = Bill.Source.Items["ForKuinID"].ToString("0").ToInt(0);
                    if (mykuin == 0) { mykuin = -1; }
                    if (mykuin == -1)
                    {
                        if (kuInIds.Count == 0)
                        {
                            Bill.Notice.intro = ("当前登记单没有关联" + (poType == 1 ? "派工" : "委外") + "入库,  所以无法修改， 建议先删除本单，入库后再登记.");
                        }
                        else
                        {
                            mykuin = kuInIds[0];
                            Bill.Source.Items["ForKuinID"] = mykuin;
                        }
                    }
                    if (mykuin > 0 && kuInIds.Contains(mykuin) == false)
                    {
                        kuInIds.Add(mykuin);
                    }
                    var ForKuinID = basegp.Fields.Add<BillSelectBoxField>("关联入库", "ForKuinID").CNotNull();
                    ForKuinID.NullText = "选择关联入库单";
                    ForKuinID.CWidth(150);
                    if (kuInIds != null && kuInIds.Count() > 0)
                    {
                        ForKuinID.SetOptionsBySql("select title,ord as ForKuinID from dbo.kuin where ord in(" + kuInIds.Join(",") + ")");
                        ForKuinID.CallBack = new BillFieldCallBackClass(BFCallBackEventEnum.Change, KuinChangeClick);
                    }
                    basegp.Fields.AddText("", "KuInTitle").CDisplay(BFDisplayEnum.Hidden);


                    if (callType == BillInitEventType.OnInit)
                    {
                        FalseDeleteRegBill(Bill.Ord);
                        materialRegStatusItems = materialRegHelper.GetMaterialRegDetailsStatusList(new int[] { mykuin });
                        Sql.RollbackTrans();
                    }
                    else
                    {
                        materialRegStatusItems = materialRegHelper.GetMaterialRegDetailsStatusList(new int[] { mykuin });
                    }

                    if (materialRegStatusItems != null && materialRegStatusItems.Count() > 0)
                    {

                        MaterialRegStatusItem model = materialRegStatusItems.FirstOrDefault();
                        BillHtmlField regedNum = basegp.Fields.Add<BillHtmlField>("入库/已登记", "KuInNumInfo").CDisplay(BFDisplayEnum.ReadOnly);
                        decimal rednum = 0.00M;
                        decimal kuinNum = 0.00m;
                        kuinNum = model.KuinNum;
                        KuInNumValue.DefValue = model.KuinNum.ToString();
                        KuInNumValue.Value = KuInNumValue.DefValue;
                        rednum = ConvertHelper.ToDecimal(model.KuinRegCompletedNum);

                        var regedNumbox = basegp.Fields.AddNumber("已登记数量", "KuinRegCompletedNum").CDisplay(BFDisplayEnum.Hidden);
                        regedNumbox.DefValue = model.KuinRegCompletedNum.ToString();
                        regedNumbox.Value = regedNumbox.DefValue;

                        regedNum.FormatHTML = string.Format("{0:N" + SystemInfoClass.NumberBit + "}", kuinNum).ToString()
                            + "/" + string.Format("{0:N" + SystemInfoClass.NumberBit + "}", rednum).ToString();
                    }
                    #endregion
                }
                BillNumberBoxField totalnum = basegp.Fields.AddNumber("待登记数量", "totalnum").CNotNull();
                totalnum.DisNegative = true;
                totalnum.Min = 0.00000001;
                if (dtl != null && dtl.Rows.Count > 0)
                {
                    totalnum.DefValue = dtl.Rows[0]["totalnum"].ToString();
                }
            }
            var OrderType = basegp.Fields.AddText("", "OrderType");
            OrderType.CDisplay(BFDisplayEnum.Hidden).CDefValue("1");
            var poTypetxt = basegp.Fields.AddText("", "poType");
            poTypetxt.CDisplay(BFDisplayEnum.Hidden);
            poTypetxt.CDefValue(poType.ToString());
            var bid = basegp.Fields.AddText("", "bid");
            bid.CDisplay(BFDisplayEnum.Hidden);
            bid.CDefValue(TextCoderClass.DeUrl(GetText("fromID")).IsNull("0"));
            var canRk = basegp.Fields.AddText("", "canRk");
            canRk.CDisplay(BFDisplayEnum.Hidden).IsNull("0");
            basegp.Fields.AddText("", "ProductID").CDisplay(BFDisplayEnum.Hidden);
            #endregion

            #region 物料登记明细
            BillFieldGroupClass progp = Bill.Groups.Add("物料登记明细", "progp");
            BillListViewField rglvw = progp.Fields.addListView("", "rglvw").CNotNull();
            ListViewClass lvw = rglvw.ListView;
            if (Bill.UiState == BillViewStateEnum.Details)
            {
                lvw.Page.PageSize = 99999;
                lvw.UI.IsHidePageBar = true;
            }

            if (dtl != null && dtl.Rows.Count > 0)
            {
                date1.DefValue = dtl.Rows[0]["date1"].ToString();
                proorder.DefValue = dtl.Rows[0]["zorder1"].ToString();
                productModel.DefValue = dtl.Rows[0]["ProductModel"].ToString();
                int poTypeV = ConvertHelper.ToInt(dtl.Rows[0]["poTypeV"].ToString());
                if (poTypeV == 3 || poTypeV == 4)
                {
                    poType = 3;
                }
                poTypetxt.DefValue = poType.ToString();
                bid.DefValue = dtl.Rows[0]["bid"].ToString();
                canRk.DefValue = dtl.Rows[0]["canRk"].ToString();

                int ProductId = ConvertHelper.ToInt(dtl.Rows[0]["zProductID"]);
                BillInfoClass Bic = BillCommModule.GetBill(16001, ProductId, dtl.Rows[0]["ztitle"].ToString());
                proname.FormatHTML = Bic.linktitle.IsNull(dtl.Rows[0]["ztitle"].ToString());
                lvw.Source.SetDataTable(dtl);
            }
            else
            {
                if (Bill.UiState == BillViewStateEnum.Change || Bill.UiState == BillViewStateEnum.Modify)
                {
                    FromKuinId = Bill.Source.Items["ForKuinID"].ToString().ToInt(-1);
                    lvw.Source.SetDataTable(GetLvwDataSqlTable(new int[] { FromKuinId }, 2));
                }
                else
                {
                    lvw.Source.MainSql = @"SELECT mrl.ID,isnull(mrl.ListID,0) ListID,p.title protitle,(case ISNULL(p.del,99) when 1 then '' when 2 then '(已删除)' when 99 then '产品已彻底删除' end) deltitle,p.order1,p.type1,mrl.unit,ISNULL(wal.num1,l.num) sx,
                                            ISNULL(cbc.num1,0) cbc,mro.num as num1,
                                            isnull((case when  isnull(p.pricemode,0) =2 and k.sort1 not in (2,8) then k2.pricemonth else k2.price1 end)* isnull(mro.num1,0)/nullif(mro.num,0),0) cbprice,
                                            (case when  isnull(p.pricemode,0) =2 and k.sort1 not in (2,8) then k2.pricemonth else k2.price1 end) * isnull(mro.num1,0) cbmoney,
                                            0 ky,mrl.zy zy,mrl.intro,
                                            k2.ph, k2.xlh , CONVERT(VARCHAR(10),k2.datesc,120) datesc,CONVERT(VARCHAR(10),k2.dateyx,120) dateyx,mrl.ProductID,mrl.BomList,
                                           CASE WHEN zy=2 THEN  mro.num ELSE 0 end mapnum,'' zdmapnum,mapping.mapping,
                                            ISNULL(ISNULL(wal.bl,bl.bl),1) bl,mrl.poTypeV,oo.gys,isnull(wa.Id,oo.ID) AS bid
                                            ,(select top 1 e.sort1 from dbo.sortonehy e where e.gate2 = 61 and e.ord=mrl.unit) unname
                                            ,mr.ForKuinID
                                            ,(CASE WHEN pdetail.qx_open = 1 THEN 1 ELSE 0 END) pdetail
                                            ,ISNULL(ISNULL(wal.WastAge,bl.PCWastage),0) NeedWastAge
                                        FROM dbo.M2_MaterialRegisterLists mrl
                                        INNER JOIN dbo.M2_MaterialRegisters mr ON mrl.MRID = mr.ID
                                        INNER JOIN dbo.M2_RegisterOccupy mro ON mrl.ID = mro.MRLID AND mro.isOld = 0
                                        LEFT JOIN dbo.product p ON mrl.ProductID = p.ord
                                        LEFT JOIN dbo.kuoutlist2 k2 ON mro.kuoutlist2 = k2.id
                                        left join dbo.kuout k on k.ord = k2.kuout
                                        LEFT JOIN dbo.M2_WorkAssigns wa ON mr.bid = wa.ID and mr.poType in(1,2)
                                        LEFT JOIN dbo.M2_WorkAssignLists wal ON wa.ID = wal.WAID AND mrl.ListID = wal.ID
                                        LEFT JOIN dbo.M2_OutOrderlists ool ON mr.bid = ool.ID
                                        LEFT JOIN dbo.M2_OutOrder oo ON ool.outID = oo.ID and mr.poType in(3,4)
                                        LEFT JOIN dbo.M2_OutOrderlists_wl l ON ool.outID = l.outID AND mrl.ListID = l.ID AND (ISNULL(ool.molist,0) = ISNULL(l.pmolist,1) OR l.pmolist IS NULL)
                                        LEFT JOIN dbo.M2_BOMList bl ON l.BomListID = bl.ID
                                        LEFT JOIN dbo.power pdetail ON pdetail.ord = " + UserInfo.UserID + @" AND pdetail.sort1 = 21 AND pdetail.sort2 = 14
                                        LEFT JOIN (
                                            SELECT mrl.ListID,mro.unit,SUM(mro.num) num1 FROM dbo.M2_MaterialRegisterLists mrl
                                            INNER JOIN dbo.M2_RegisterOccupy mro ON mrl.ID = mro.MRLID AND mro.isOld = 0
                                            INNER JOIN dbo.M2_MaterialRegisters mr ON mrl.MRID = mr.ID AND mr.del = 1
                                            WHERE mr.OrderType = 1 AND mrl.MRID <> @ord
                                            AND ISNULL(mr.status,-1) <> 0                                
                                            GROUP BY mrl.ListID,mro.unit
                                        ) cbc ON ISNULL(wal.ID,ool.ID) = cbc.ListID and cbc.unit = ISNULL(wal.unit,ool.unit1)
                                        LEFT JOIN (
	                                        SELECT
                                            MRLID,
                                            mapping = (STUFF((SELECT '|' + CAST(kuoutlist2 AS VARCHAR(20))+','+ CAST(num AS VARCHAR(20))
                                                             FROM M2_RegisterOccupy
                                                             WHERE MRLID = A.MRLID
                                                             FOR xml path('')),1,1,''))
	                                        FROM M2_RegisterOccupy A WHERE A.MRLID IN(SELECT ID FROM dbo.M2_MaterialRegisterLists WHERE MRID = @ord)
	                                        GROUP by A.MRLID
                                        ) mapping ON mrl.ID = mapping.MRLID
                                        where mrl.MRID = @ord and isnull(mrl.Mergeinx,0) <= 0
                                        order by mrl.ID asc";
                }
            }

            ListViewHeader column = new ListViewHeader();
            column = lvw.Headers["protitle"].CTitle("物料名称").CUIType(FieldUITypeEnum.TextBox).CNotNull().CAlign("left");
            column.ShowHandleBar = true;
            column.CanBatchInput = false;
            column.CAutoComplete(
                "../Material/SurplusNumber.ashx?RegId="+Bill.Ord+"&auto=1&OutSourcestatus=0&FilterRegForkuinId=@[ForKuinID]", AutoCompleteTriggerEnum.Both,
                  true
            );

            lvw.Headers["deltitle"].CVisible(false);
            if (lvw.Headers["pdetail"] != null)
            {
                lvw.Headers["pdetail"].CVisible(false);
            }
            if (Bill.UiState == BillViewStateEnum.Details)
            {
                column.FormatHtml = "script:app.CLinkHtml('@protitle<span style=\"color:red;\">@deltitle</span>', '" + this.VirPath + "SYSA/product/content.asp?ord='+ app.pwurl(@ProductID) +'', 1,@pdetail)";
            }
            else
            {
                column.DynAttributes.Add("display", "@ListID>0?'readonly':'editable'");
            }
            lvw.Headers["order1"].CTitle("物料编号");
            lvw.Headers["type1"].CTitle("物料型号");
            column = lvw.Headers["unit"].CTitle("单位").CCanBatchInput(false);
            column.CType<BillSelectBoxField>().CNullText("");
            column.CNotNull().SetOptions(SQLSortonehyTypeEnum.SH_产品单位);
            column.CDisplay(BFDisplayEnum.ReadOnly).CanSave = true;
            var sx = lvw.Headers["sx"].CTitle("所需数量");
            sx.CUIType(FieldUITypeEnum.NumberBox);
            sx.CDisplay(BFDisplayEnum.ReadOnly);
            sx.Visible = Bill.UiState != BillViewStateEnum.Details;

            lvw.Headers["cbc"].CDisplay(BFDisplayEnum.Hidden).CanSave = true;

            column = lvw.Headers["num1"].CTitle("本次登记数量").CCanBatchInput(false).CUIType(FieldUITypeEnum.NumberBox).CNotNull();
            column.Min = 0.00000001;
            column.CanSave = true;
            column.CanSum = true;

            if (Bill.UiState != BillViewStateEnum.Details)
            {
                column.Formula = "@ListID > 0 ? (MainFieldVal('totalnum') != '' ? (@bl > 0 ? ((MainFieldVal('totalnum') * @bl*(1+@NeedWastAge*0.01)) < (@sx) ? MainFieldVal('totalnum') * @bl*(1+@NeedWastAge*0.01) : @sx) : @sx) : @sx) :'抛弃我吧!'";
                column.FormulaTiggerDBNames = "MainFieldVal('totalnum')";
            }
            if (Bill.UiState == BillViewStateEnum.Details && SystemPowerClass.ExistsPower(SQLPowerTypeEnum.库存权限, 2))
            {
                lvw.Headers["cbprice"].CTitle("成本单价").CUIType(FieldUITypeEnum.MoneyBox).CApplyType(BillMoneyApplyEnum.StorePrice).CanSum = false;
                lvw.Headers["cbmoney"].CTitle("成本总价").CUIType(FieldUITypeEnum.MoneyBox).CanSum = true;
            }
            else
            {
                lvw.Headers["cbprice"].CDisplay(BFDisplayEnum.Hidden);
                lvw.Headers["cbmoney"].CDisplay(BFDisplayEnum.Hidden);
            }
            lvw.Headers["unname"].CDisplay(BFDisplayEnum.Hidden);
            column = lvw.Headers["ky"].CTitle("可用数量").CUIType(FieldUITypeEnum.NumberBox).CUnit("@unname");
            column.CDisplay(BFDisplayEnum.ReadOnly);
            column.CCanSave();
            column.Visible = Bill.UiState != BillViewStateEnum.Details;
            if (Bill.UiState == BillViewStateEnum.Modify || Bill.UiState == BillViewStateEnum.Change)
            {
                column.FormatServer = GetkyNumInfo;
            }

            lvw.Headers["mapnum"].CDisplay(BFDisplayEnum.Hidden).CanSave = true;
            lvw.Headers["zdmapnum"].CDisplay(BFDisplayEnum.Hidden).CanSave = true;
            column = lvw.Headers["zy"].CTitle("分配方式").CUIType(FieldUITypeEnum.RadioBoxs).CDefValue("1");
            column.StatusFormatHtml = "<div class=\"zd\" style=\"color: red\">@zdmapnum</div>";
            column.SetOptions("随机=1,指定=2");
            column.CanSave = true;
            column.Visible = Bill.UiState != BillViewStateEnum.Details;
            column.CallBack = new BillFieldCallBackClass(ZYClickCallBack, BFCallBackEventEnum.Click);

            lvw.Headers["intro"].CTitle("备注").CUIType(FieldUITypeEnum.TextArea).CAlign("left").CMax(500).CanSave = true;
            if (Bill.UiState == BillViewStateEnum.Details && SystemPowerClass.ExistsModule(17003))
            {
                lvw.Headers["ph"].CTitle("批号");
                lvw.Headers["xlh"].CTitle("序列号");
                lvw.Headers["datesc"].CTitle("生产日期").CType<BillDateBoxField>().DateUI = BillDateBoxUIEnum.Date;
                lvw.Headers["dateyx"].CTitle("有效日期").CType<BillDateBoxField>().DateUI = BillDateBoxUIEnum.Date;
            }
            else
            {
                lvw.Headers["ph"].CDisplay(BFDisplayEnum.Hidden);
                lvw.Headers["xlh"].CDisplay(BFDisplayEnum.Hidden);
                lvw.Headers["datesc"].CDisplay(BFDisplayEnum.Hidden);
                lvw.Headers["dateyx"].CDisplay(BFDisplayEnum.Hidden);
            }
            lvw.Headers["BomList"].CDisplay(BFDisplayEnum.Hidden).CanSave = true;
            lvw.Headers["ProductID"].CDisplay(BFDisplayEnum.Hidden).CanSave = true;
            lvw.Headers["ListID"].CDisplay(BFDisplayEnum.Hidden).CanSave = true;
            lvw.Headers["bl"].CDisplay(BFDisplayEnum.Hidden).CanSave = true;
            lvw.Headers["NeedWastAge"].CDisplay(BFDisplayEnum.Hidden).CanSave = true;
            lvw.Headers["mapping"].CDisplay(BFDisplayEnum.Hidden).CanSave = true;
            lvw.Headers["poTypeV"].CDisplay(BFDisplayEnum.Hidden).CanSave = true;

            lvw.Headers["gys"].CDisplay(BFDisplayEnum.Hidden).CanSave = true;
            var ForKuinIDhid = lvw.Headers["ForKuinID"];
            ForKuinIDhid.Formula = "MainFieldVal('ForKuinID')";
            ForKuinIDhid.CDisplay(BFDisplayEnum.Hidden).CanSave = true;

            var dbnames = new string[] {
                "billtitle",  "BH", "Status", "ztitle", "zorder1",  "gys",
                "iden",  "billID", "KuInTitle", "PGNum", "ProductModel",
                "canRk", "date1", "zProductID"
            };
            foreach (var dbname in dbnames)
            {
                if (lvw.Headers[dbname] != null)
                {
                    lvw.Headers[dbname].CDisplay(BFDisplayEnum.Hidden);
                }
            }

            if (lvw.Headers["bid"] == null)
                lvw.Headers.Add("bid", "bid").CDisplay(BFDisplayEnum.Hidden).CanSave = true;
            else
                lvw.Headers["bid"].CDisplay(BFDisplayEnum.Hidden).CanSave = true;
            if (lvw.Headers["newbid"] != null)
            {
                lvw.Headers["newbid"].CDisplay(BFDisplayEnum.Hidden).CanSave = true;
            }
            if (lvw.Headers["KuInNum"] != null)
            {
                lvw.Headers["KuInNum"].CDisplay(BFDisplayEnum.Hidden).CanSave = true;
            }
            if (lvw.Headers["KuinRegCompletedNum"] != null)
            {
                lvw.Headers["KuinRegCompletedNum"].CDisplay(BFDisplayEnum.Hidden).CanSave = true;
            }
            if (lvw.Headers["totalnum"] != null)
            {
                lvw.Headers["totalnum"].CDisplay(BFDisplayEnum.Hidden).CanSave = true;
            }
            lvw.UI.CurrSum = false;
            lvw.UI.AllSum = true;
            lvw.UI.IsAbsWidth = true;
            lvw.UI.CheckBox = Bill.UiState != BillViewStateEnum.Details;
            lvw.UI.CanDelete = true;
            lvw.UI.CanAdd = true;
            lvw.UI.CanInsert = true;
            lvw.UI.CanMove = true;
            lvw.UI.FixedCols = 3;
            #endregion

            if (Bill.UiState != BillViewStateEnum.Details)
            {
                BillFieldGroupClass othergp = Bill.Groups.Add("其他信息", "othergp");
                othergp.Fields.AddEditor("概要", "intro");
            }
            else
            {
                RelevanceClass.LoadNoticeGroup();
                RelevanceClass.LoadProduceGroup();
                RelevanceClass.LoadApproveLogGroup();
                RelevanceClass.LoadChangeRecord();
            }
            if (GetInt("fromType") > 0 && string.IsNullOrWhiteSpace(GetText("fromID")) == false)
            {
                poType = GetInt("fromType");
                if (poType == 1 || poType == 2)
                {
                    poType = 1;
                }
                else
                {
                    poType = 3;
                }
                List<BillCustomInheritType> InheritTypes = new List<BillCustomInheritType>();
                BillCustomInheritType CIT = new BillCustomInheritType();
                CIT.InheritIdBillType = BillApplyTypeEnum.B54002_生产派工单;
                CIT.InheritIdListType = 2;
                CIT.InheritFieldKey = "listID";
                CIT.RowFilterCode = "poTypeV=1";
                InheritTypes.Add(CIT);
                CIT = new BillCustomInheritType();
                CIT.InheritIdBillType = BillApplyTypeEnum.B54005_生产返工单;
                CIT.InheritIdListType = 1;
                CIT.InheritFieldKey = "listID";
                CIT.RowFilterCode = "poTypeV=2";
                InheritTypes.Add(CIT);
                switch (poType)
                {
                    case 1: BillCustomFieldsHelper.LoadListViewInheritIdsData(lvw, BillApplyTypeEnum.B55004_生产物料登记, 1, InheritTypes, 0, true); break;
                    case 2: BillCustomFieldsHelper.LoadListViewInheritIdsData(lvw, BillApplyTypeEnum.B55004_生产物料登记, 1, InheritTypes, 0, true); break;
                    case 3: BillCustomFieldsHelper.LoadListViewInheritIdData(lvw, BillApplyTypeEnum.B55004_生产物料登记, 1, BillApplyTypeEnum.B54003_整单委外, 2, "ListID", 0); break;
                    case 4: BillCustomFieldsHelper.LoadListViewInheritIdData(lvw, BillApplyTypeEnum.B55004_生产物料登记, 1, BillApplyTypeEnum.B54006_生产工序委外单, 2, "ListID", 0); break;
                }
            }
        }

        protected override void Bill_OnAutoCompleteCallBack(BillAutoCompleteEventDataClass eventdata)
        {
            if (eventdata.IsListView)
            {
                ProductChangeCallBack(eventdata);
            }
        }

        /// <summary>修改模式下：入库更改时事件 </summary>
        /// <param name="CurrData"></param>
        public void KuinChangeClick(BillSaveDataCollection CurrData)
        {
            if (Bill.Ord == 0) { return; }
            CallBacker.ListView.ClearRows("rglvw");
            int poType = ConvertHelper.ToInt(CurrData["poType"].Value);
            int tabIndex = 0;
            if (poType == 3 || poType == 4)
            {
                tabIndex = 1;
            }
            string ForKuinID = CurrData["ForKuinID"].Value;
            if (string.IsNullOrWhiteSpace(ForKuinID))
            {
                ForKuinID = "0";
            }
            FromKuinId = ForKuinID.ToInt();
            FalseDeleteRegBill(Bill.Ord);
            DataTable adddt = GetLvwDataSqlTable(ForKuinID.ToIntList(), tabIndex);
            Sql.RollbackTrans();
            if (adddt != null && adddt.Rows.Count > 0)
            {
                CallBacker.UpdateFieldValue("bid", adddt.Rows[0]["bid"].ToString());
                CallBacker.UpdateFieldValue("poTypeV", adddt.Rows[0]["poTypeV"].ToString());
                CallBacker.UpdateFieldValue("ky", adddt.Rows[0]["ky"].ToString());
                CallBacker.UpdateFieldValue("PGNum", adddt.Rows[0]["PGNum"].ToString());
                CallBacker.UpdateFieldValue("KuInNum", adddt.Rows[0]["KuInNum"].ToString());
                CallBacker.UpdateFieldValue("KuinRegCompletedNum", adddt.Rows[0]["KuinRegCompletedNum"].ToString());
                CallBacker.UpdateFieldValue("totalnum", adddt.Rows[0]["totalnum"].ToString());
                CallBacker.UpdateFieldValue("canRk", adddt.Rows[0]["canRk"].ToString());
                CallBacker.UpdateFieldValue("date1", adddt.Rows[0]["date1"].ToString());


                BillHtmlField regedNum = CallBacker.NewBill.Groups[0].Fields.Add<BillHtmlField>("入库/已登记", "KuInNumInfo").CDisplay(BFDisplayEnum.ReadOnly);
                decimal rednum = ConvertHelper.ToDecimal(adddt.Rows[0]["KuinRegCompletedNum"].ToString());
                decimal kuinNum = ConvertHelper.ToDecimal(adddt.Rows[0]["KuInNum"].ToString());
                regedNum.FormatHTML = string.Format("{0:N" + SystemInfoClass.NumberBit + "}", kuinNum).ToString()
                    + "/" + string.Format("{0:N" + SystemInfoClass.NumberBit + "}", rednum).ToString();

                CallBacker.UpdateField("KuInNumInfo");
                string PGNumName = "委外";
                if (tabIndex == 0)
                {
                    PGNumName = "派工";
                }
                BillNumberBoxField pGNum = CallBacker.NewBill.Groups[0].Fields.AddNumber(PGNumName + "数量", "PGNum").CDisplay(BFDisplayEnum.ReadOnly);
                CallBacker.UpdateField("PGNum");

                List<BillCustomInheritType> InheritTypes = new List<BillCustomInheritType>();
                BillCustomInheritType CIT = new BillCustomInheritType();
                CIT.InheritIdBillType = BillApplyTypeEnum.B54002_生产派工单;
                CIT.InheritIdListType = 2;
                CIT.InheritFieldKey = "listID";
                CIT.RowFilterCode = "poTypeV=1";
                InheritTypes.Add(CIT);
                CIT = new BillCustomInheritType();
                CIT.InheritIdBillType = BillApplyTypeEnum.B54005_生产返工单;
                CIT.InheritIdListType = 1;
                CIT.InheritFieldKey = "listID";
                CIT.RowFilterCode = "poTypeV=2";
                InheritTypes.Add(CIT);
                switch (poType)
                {
                    case 1: adddt = BillCustomFieldsHelper.LoadListViewInheritIdsData(adddt, BillApplyTypeEnum.B55004_生产物料登记, 1, InheritTypes, 0, true); break;
                    case 2: adddt = BillCustomFieldsHelper.LoadListViewInheritIdsData(adddt, BillApplyTypeEnum.B55004_生产物料登记, 1, InheritTypes, 0, true); break;
                    case 3: adddt = BillCustomFieldsHelper.LoadListViewInheritIdData(adddt, BillApplyTypeEnum.B55004_生产物料登记, 1, BillApplyTypeEnum.B54003_整单委外, 2, "ListID", 0); break;
                    case 4: adddt = BillCustomFieldsHelper.LoadListViewInheritIdData(adddt, BillApplyTypeEnum.B55004_生产物料登记, 1, BillApplyTypeEnum.B54006_生产工序委外单, 2, "ListID", 0); break;
                }
            }
            CallBacker.ListView.AddRows("rglvw", adddt);
            CallBacker.RefreshListView("rglvw");
        }

        /// <summary>明细编辑行： 产品选择修改事件</summary>
        public static void ProductChangeCallBack(BillAutoCompleteEventDataClass callevendata)
        {

			if()
            var kuoutlists = new List<RowIndexData<int>>();
            int rowindex = 0;
            foreach (var row in callevendata.AutoCompleteReturnDatas)
            {
                kuoutlists.Add(new RowIndexData<int>
                {
                    RowIndex = rowindex,
                    Data = row["id"].ToString("0").ToInt()
                });
                rowindex++;
            };
            //查询返回数据
            var datas =
            DBQuery.CTempTable(
                kuoutlists
            ).Inner_Join<kuoutlist2>(
                (x, y) => x.Data == y.Id
            ).Inner_Join<Comm_Product>(
                (x, y, z) => z.Ord == y.Ord
            ).Inner_Join<Sortonehy>(
                (x, y, z, m) => y.Unit == m.Ord
            )
            .Select(
                (x, y, z, m) => new
                {
                    ProductID = y.Ord,
                    protitle = z.Title,
                    order1 = z.Order1,
                    type1 = z.Type1,
                    unit = y.Unit,
                    intro = "",
                    rowIndex = x.RowIndex,
                    unname = m.Sort1,
                    ForKuinID = 0,
                    num1 = y.Numleft,
                    zdmapnum = "",
                    mapping = x.Data
                }, -1, "rowIndex"
            ).GetDataList();

            //查询总的可用库存
            var kuinid = callevendata.CurrCallBacker.SaveDatas["ForKuinID"].Value.ToString("0").ToInt();
            if (kuinid == 0)
            {
                kuinid = ConvertHelper.ToInt(callevendata.CurrRowData["ForKuinID"]);
            }
            var query = new ZBServices.flib.Produce.Material.MaterialRegStatusQueryClass();
            query.KuinIds = new int[] { kuinid };
            query.DisKuinStatusFilter = true;
            query.OutersMaterialRegDetailsList = new List<MaterialRegWaitUpdaterItem>();
            foreach (var dat in datas)
            {
                query.OutersMaterialRegDetailsList.Add(new MaterialRegWaitUpdaterItem
                {
                    ForKuinId = kuinid,
                    ProductID = dat.ProductID,
                    Unit = dat.unit,
                    NeedRegNum = dat.num1
                });
            }
            var kuinfos = query.GetMaterialRegDetailsStatusListData(true);
            query = null;
            var lvwid = callevendata.LvwDBName;
            rowindex = callevendata.LvwRowIndex;
            var colindex = callevendata.LvwColIndex;
            var currptitledata = callevendata.CurrRowData["protitle"];
            var istreelist = currptitledata is IDictionary;


            //创建新空行占位
            DataTable nulldb = new DataTable();
            nulldb.Columns.Add("protitle");
            if (istreelist) { nulldb.Columns.Add("bid"); }
            for (var x = 1; x < datas.Count; x++)
            {
                if (istreelist)
                {
                    nulldb.Rows.Add("", callevendata.CurrRowData["bid"].ToString());
                }
                else
                {
                    nulldb.Rows.Add("");
                }
            }
            var CallBacker = callevendata.CurrCallBacker;
            CallBacker.ListView.AddRows(lvwid, nulldb, "", "", rowindex);

            //输出结果到前台
            for (var ii = 0; ii < datas.Count; ii++)
            {
                var dat = datas[ii];
                var ky = (from item in kuinfos
                          where item.MaterialProductID == dat.ProductID
                          && item.KuinId == kuinid && item.MaterialUnitID == dat.unit
                          select item.MaterialRoomKuNum).FirstOrDefault();

                var currRowindex = (rowindex + dat.rowIndex).ToString();
                CallBacker.ListView.UpdateCellsValue(lvwid, currRowindex, "ProductID", dat.ProductID.ToString(), true);
                if (currptitledata is IDictionary)  //currptitledata is IDictionary  表明批量登记下，树节点
                {
                    var dkey = currptitledata as Dictionary<string, object>;
                    dkey["text"] = dat.protitle;
                    dkey["id"] = dat.ProductID;
                    dkey["next"] = ii == datas.Count - 1 ? 1 : 0;
                    CallBacker.ListView.UpdateCellsValue(lvwid, currRowindex, "protitle", "__json:" + JSONClass.GetJSON(dkey), true);
                    CallBacker.ListView.UpdateCellsValue(lvwid, currRowindex, "bid", callevendata.CurrRowData["bid"].ToString(), true);
                }
                else
                {
                    CallBacker.ListView.UpdateCellsValue(lvwid, currRowindex, "protitle", dat.protitle.ToString(), true);
                }
                CallBacker.ListView.UpdateCellsValue(lvwid, currRowindex, "order1", dat.order1.ToString(), true);
                CallBacker.ListView.UpdateCellsValue(lvwid, currRowindex, "type1", dat.type1.ToString(), true);
                CallBacker.ListView.UpdateCellsValue(lvwid, currRowindex, "unit", dat.unit.ToString(), true);
                CallBacker.ListView.UpdateCellsValue(lvwid, currRowindex, "intro", "", true);
                CallBacker.ListView.UpdateCellsValue(lvwid, currRowindex, "unname", dat.unname.ToString(), true);
                CallBacker.ListView.UpdateCellsValue(lvwid, currRowindex, "zy", "2", true);
                CallBacker.ListView.UpdateCellsValue(lvwid, currRowindex, "ky", ky.ToString(), true);
                CallBacker.ListView.UpdateCellsValue(lvwid, currRowindex, "num1", dat.num1.ToString(), true);
                CallBacker.ListView.UpdateCellsValue(lvwid, currRowindex, "mapnum", dat.num1.ToString(), true);
                CallBacker.ListView.UpdateCellsValue(lvwid, currRowindex, "zdmapnum", "已指定：" + dat.num1.ToString(NumberBitEnum.Number), true);
                CallBacker.ListView.UpdateCellsValue(lvwid, currRowindex, "mapping", dat.mapping.ToString() + "," + dat.num1.ToString() + "," + dat.unit + "|", true);
                CallBacker.ListView.UpdateCellsValue(lvwid, currRowindex, "ForKuinID", dat.ForKuinID.ToString(), true);
            }
            CallBacker.RefreshListView(lvwid);

        }

        /// <summary>明细编辑行： 指定或随机点击回调事件</summary>
        public void ZYClickCallBack(ListViewCellCallBackClass cell)
        {
            if (cell.Row != null)
            {
                if (cell.Row["zy"].ToString() == "2")
                {
                    var mapnum = cell.Row["mapnum"].IsNull("0").ToDecimal();
                    var mapping = cell.Row["mapping"].IsNull("0");
                    var ProductID = cell.Row["ProductID"].ToString().ToInt().ToBillOrderUrl();
                    var unit = cell.Row["unit"].ToString().ToInt().ToBillOrderUrl();
                    var n = cell.Row["num1"].ToString().ToDecimal();
                    var gys = cell.Row["gys"].IsNull("0").ToInt().ToBillOrderUrl();
                    var listId = cell.Row["ListID"].IsNull("0").ToInt().ToBillOrderUrl();
                    var poTypeV = cell.Row["poTypeV"].IsNull("0").ToInt().ToBillOrderUrl();

                    var bid = cell.Row["bid"].IsNull("0").ToInt().ToBillOrderUrl();
                    string KuoutIds = cell.Row["ForKuinID"].IsNull("0").ToInt().ToBillOrderUrl();

                    if (mapnum>0) {
                        cell.SetListViewCellValue("zdmapnum", "已指定：" + mapnum.ToString(NumberBitEnum.Number));
                    }
                    cell.AddScript("app.OpenUrl('" + GetVirPath + "SYSN/view/produceV2/MaterialRegisters/RegisterOccupy.ashx?RegID=' + Bill.Data.ord + '&KuinIds=" + KuoutIds + "&proid=" + ProductID + "&unit=" + unit + "&n=" + n + "&r=" + cell.RowIndex + "&g=" + gys + "&listId=" + listId + "&poType=" + poTypeV + "&bid=" + bid + "&mapnum = " + ConvertHelper.ToInt(mapnum).ToBillOrderUrl() + "')");
                }
                else
                {
                    cell.SetListViewCellValue("mapnum", "0");
                    cell.SetListViewCellValue("mapping", "");
                    cell.SetListViewCellValue("zdmapnum", "");
                }
            }
            else
            {
                cell.SetListViewCellValue("mapnum", "0");
                cell.SetListViewCellValue("mapping", "");
                cell.SetListViewCellValue("zdmapnum", "");
            }
        }

        /// <summary>
        /// 获取单据状态
        /// </summary>
        /// <param name="poType">1:派工3：整单委外</param>
        /// <param name="billID">单据ID</param>
        /// <returns></returns>
        public string GetMaterialRegStatusName(int poType, int billID)
        {
            string statusName = "";
            int RegStatus = 0;
            if (poType == 3)
            {
                RegStatus = MaterialRegHelper.Instance.GetMaterialRegStatus(UserInfo.UserID, MaterialRegFromBillType.B_整单委外单, billID);
            }
            else
            {

                RegStatus = MaterialRegHelper.Instance.GetMaterialRegStatus(UserInfo.UserID, MaterialRegFromBillType.B_派工单, billID);
            }
            switch (RegStatus)
            {
                case 0:
                    statusName = "未登记";
                    break;
                case 1:
                    statusName = "部分登记";
                    break;
                case 2:
                    statusName = "登记完毕";
                    break;
                case 3:
                    statusName = "超量登记";
                    break;
                case 4:
                    statusName = "无需登记";
                    break;
                default:
                    break;
            }
            return statusName;
        }

        protected override void Bill_OnLoadComplete()
        {

            if (Bill.UiState == BillViewStateEnum.Details)
            {
                if (CostAnalysisModule.IsCostAnalysis(BillApplyTypeEnum.B55004_生产物料登记, Bill.Ord, ""))
                {
                    Bill.CommandButtons.Remove("bill.change");
                }
            }
        }

        #endregion

        #region  单据保存、删除、变更数据存储
        public override void Bill_OnSave(BillSaveDataCollection SaveDatas)
        {
            #region 保存校验处理

            if (string.IsNullOrWhiteSpace(SaveDatas["billBH"].Value.IsNull("")))
            {
                Sql.RollbackTrans();
                CallBacker.MessageBox("登记信息没有给关联单据！");
                return;
            }
            int kuinid = SaveDatas["ForKuinID"].Value.ToString("0").ToInt();
            if (KuIn.Where(x => x.Ord == kuinid && x.Del == 1).Select(x => new { x.Ord }).Count() == 0)
            {
                CallBacker.MessageBox("入库单已删除！");
                return;
            }

            //BUG:36386
            decimal KuInNum = ConvertHelper.ToDecimal(SaveDatas["KuInNum"].Value);
            decimal KuinRegCompletedNum = ConvertHelper.ToDecimal(SaveDatas["KuinRegCompletedNum"].Value);
            decimal newKuinRegCompletedNum = MaterialRegHelper.Instance.GetKuinRegCompletedNum(kuinid, Bill.Ord);
            decimal totalnum = ConvertHelper.ToDecimal(SaveDatas["totalnum"].Value);
            if ((totalnum + newKuinRegCompletedNum) > KuInNum)
            {
                Sql.RollbackTrans();
                string addmsg = "";
                if (newKuinRegCompletedNum > KuinRegCompletedNum)
                {
                    addmsg = "已有其它用户进行了本入库单的登记操作。";
                }
                CallBacker.MessageBox("超过入库数量！" + addmsg);
                return;
            }

            if (Sql.Exists("SELECT 1 FROM dbo.M2_CostComputation cc WHERE cc.complete1 = 1 AND DATEDIFF(MONTH,cc.date1,'" + ConvertHelper.ToDate(SaveDatas["date1"].Value) + "') = 0 "))
            {
                Sql.RollbackTrans();
                CallBacker.MessageBox("登记日期所在月已核算，请修改！");
                return;
            }
            //判断，编号不能重复
            if (Sql.Exists("SELECT TOP 1 1 FROM dbo.M2_MaterialRegisters WHERE del = 1 AND OrderType = 1 AND MOBH = '" + SaveDatas["MOBH"].Value.AsSQLText() + "'" + (Bill.Ord > 0 ? "AND ID <> " + Bill.Ord + "" : "") + ""))
            {
                Sql.RollbackTrans();
                CallBacker.ShowVerificationText("MOBH", "登记编号已存在！");
                return;
            }

            DataTable rglvw = SaveDatas["rglvw"].Table;
            Sql.CreateSqlTableByDataTable("#wllvwtable", rglvw, true);
            decimal error_v = BillCommModule.GetErrorValue(SystemInfoClass.NumberBit);
            var gys = 0;
            var nodebid = 0;
            if (rglvw != null && rglvw.Rows.Count > 0)
            {
                if (rglvw.Rows[0]["gys"].ToString() != "")
                {
                    gys = Convert.ToInt32(rglvw.Rows[0]["gys"].ToString());
                }
                if (rglvw.Rows[0]["bid"].ToString() != "")
                {
                    nodebid = Convert.ToInt32(rglvw.Rows[0]["bid"].ToString());
                }
            }

            List<int> zylist = new List<int>();
            List<int> sjlist = new List<int>();
            List<int> ctlist = new List<int>();
            int i = 0;
            foreach (DataRow item in rglvw.Rows)
            {
                var num1 = ConvertHelper.ToDecimal(item["num1"].ToString());
                var ky = ConvertHelper.ToDecimal(item["ky"].ToString());
                var mapnum = ConvertHelper.ToDecimal(item["mapnum"].ToString());

                if (ConvertHelper.ToInt(item["zy"].ToString()) == 2)
                {
                    if (Math.Abs(num1 - mapnum) > error_v)
                    {
                        zylist.Add(i);
                    }
                }
                else
                {
                    if (num1 > 0 && Math.Abs(num1 - ky) > error_v
                        && num1 > ky)
                    {
                        sjlist.Add(i);
                    }
                }
                var totalnum1 = ConvertHelper.ToDecimal(SaveDatas["totalnum"].Value);
                var bl = ConvertHelper.ToDecimal(item["bl"].ToString());
                var WastAge = ConvertHelper.ToDecimal(item["NeedWastAge"].ToString());
                var currnum = ConvertHelper.ToDecimal(item["num1"].ToString());
                if (Math.Abs(totalnum1 * bl * (1 + WastAge / 100) - currnum) > error_v)
                {
                    ctlist.Add(i);
                }

                i++;
            }
            if (zylist.Count > 0)
            {
                Sql.RollbackTrans();
                CallBacker.ListView.ShowCellsVerifyInfo("rglvw", string.Join(",", zylist), "zy", "与本次登记数量不匹配");
                return;
            }
            if (sjlist.Count > 0)
            {
                Sql.RollbackTrans();
                CallBacker.ListView.ShowCellsVerifyInfo("rglvw", string.Join(",", sjlist), "num1", "可用数量不足");
                return;
            }
            if (ctlist.Count > 0)
            {
                if (!CallBacker.Question("登记物料数量不成套，您确定要登记吗？"))
                {
                    Sql.RollbackTrans();
                    return;
                }
            }
            string v = Sql.GetValues(@"SELECT row_index-1 FROM #wllvwtable tb
                                        left JOIN (
                                                SELECT tb.ProductID,tb.unit,tb.ForKuinID,(SUM(cast(ISNULL(tb.num1,0) as decimal(25,12)))-isnull(tb.ky,0)) numleft 
                                                FROM #wllvwtable tb
		                                        GROUP BY tb.ProductID,tb.unit,tb.ForKuinID,tb.ky
                                            ) tl ON tb.ProductID = tl.ProductID and tb.ForKuinID = tl.ForKuinID and tb.unit=tl.unit
                                            WHERE tb.zy = 1 and tl.numleft>" + error_v);
            if (!string.IsNullOrWhiteSpace(v))
            {
                Sql.RollbackTrans();
                CallBacker.ListView.ShowCellsVerifyInfo("rglvw", v, "num1", "可用数量不足");
                return;
            }
            #endregion

            #region 修改模式，释放库存占用，(保存后会重新占用）
            if (Bill.Ord > 0)
            {
                var ids = M2_MaterialRegisterLists.Where(x => x.MRID == Bill.Ord).Select(x => new { x.ID }).GetValuesList<int>();
                if (ids.Length > 0)
                {
                    var kuoutlists = M2_RegisterOccupy.Where(x => ids.Contains(x.MRLID)).Select(x => new { x.Kuoutlist2 }).GetValuesList<int>();
                    Sql.Execute("delete  M2_RegisterOccupy where  MRLID IN(" + string.Join(",", ids) + ")");
                    MaterialHelper.Instance.ReAnalysisRoomKuInfo(kuoutlists);
                }

            }
            #endregion

            if (SaveDatas.SaveToDataBase() > 0)
            {
                #region 分配方式指定处理
                DataTable mapping = Sql.GetTable("SELECT ID,ProductID,unit,mapping FROM dbo.M2_MaterialRegisterLists WHERE zy = 2 AND LEN(ISNULL(mapping,'')) > 0 AND MRID = " + Bill.Ord);
                if (mapping.Rows.Count > 0)
                {
                    Sql.Execute("DELETE FROM dbo.M2_RegisterOccupy WHERE isOld = 0 AND MRLID IN(SELECT ID FROM dbo.M2_MaterialRegisterLists WHERE zy = 2 AND LEN(ISNULL(mapping,'')) > 0 AND MRID = " + Bill.Ord + ")");
                    foreach (DataRow dr in mapping.Rows)
                    {
                        int proid = dr["ProductID"].ToString().ToInt();
                        int punit = dr["unit"].ToString().ToInt();
                        string[] m = dr["mapping"].ToString().Split('|');
                        foreach (var item in m)
                        {
                            if (!string.IsNullOrWhiteSpace(item))
                            {
                                string[] it = item.Split(',');
                                if (it.Length >= 3)
                                {
                                    decimal blnum = Math.Round(Sql.GetValue("select top 1 dbo.erp_comm_UnitConvert(" + proid + "," + punit + "," + it[1] + ",k2.unit) from dbo.kuoutlist2 k2 where id=" + it[0]).IsNull("1").ToDecimal(), SystemInfoClass.NumberBit);
                                    //isold为了登记删除的时候撤回数据
                                    Sql.Execute("INSERT INTO dbo.M2_RegisterOccupy(MRLID,kuoutlist2,unit,num,isOld,unit1,num1)VALUES(" + dr["ID"].ToString() + "," + it[0] + "," + punit + "," + it[1] + ",0," + it[2] + "," + blnum + ")");
                                }
                            }
                        }
                    }
                    //校验多行指定同一条出库数据时数量不够的情景
                    if (Sql.Exists(@"SELECT TOP 1 1 FROM (
                        SELECT k2.id,k2.numleft,SUM(mro.num1) num1 FROM dbo.M2_MaterialRegisterLists mrl
                        INNER JOIN dbo.M2_RegisterOccupy mro ON mrl.ID = mro.MRLID AND mro.isOld = 0
                        INNER JOIN dbo.kuoutlist2 k2 ON mro.kuoutlist2 = k2.id
                        WHERE mrl.zy = 2 AND mrl.MRID = " + Bill.Ord + @"
                        GROUP BY k2.id,k2.numleft
                        ) tb WHERE (tb.numleft+"+ error_v + @") < tb.num1"))
                    {
                        Sql.RollbackTrans();
                        CallBacker.MessageBox("物料登记明细中指定数量总数已超过可指定数量总数！");
                        return;
                    }
                }
                #endregion
                #region 物料登记释放库存
                try
                {
                    Sql.Execute("update [M2_MaterialRegisters] set status=1 where status is null and id=" + Bill.Ord);  //默认需要设置为待审批状态， 否则可能是NULL值
                    MaterialRegHelper.Instance.ReleaseRoomKuInfo(new int[] { Bill.Ord });
                }
                catch (NoEnoughRoomStockException ex)
                {
                    var productinfos = Sql.GetColValues(@"select  
								y.title + '【' + y.order1 + '】'
							from M2_MaterialRegisterLists  x
							inner join product y on x.productID=y.ord
							where x.id in (" + ex.RegListIDs.Join(",") + ")");
                    Sql.RollbackTrans();
                    CallBacker.MessageBox("原材料“" + string.Join(",", productinfos) + "”可用数量不足。\n\n建议您检查一下： \n1、当前单据是否存在多个相同产品占用同一库存 \n2、是否存在其它用户已进行相同登记操作 ");
                    return;
                }
                catch (NoRoomKuException ex)
                {
                    var productinfos = Sql.GetColValues(@"select  
								y.title + '【' + y.order1 + '】'
							from kuoutlist2  x
							inner join product y on x.ord=y.ord
							where x.id in (" + ex.KuoutLists.Select(x => x.KuoutListID).ToList().Join(",") + ")");
                    Sql.RollbackTrans();
                    CallBacker.MessageBox("原材料“" + string.Join(",", productinfos) + "”可用数量不足, 无法扣除， 请在车间剩余表中检查该产品剩余信息是否正确。");
                    return;
                }
                catch (Exception ex)
                {
                    Sql.RollbackTrans();
                    CallBacker.MessageBox("可用数量不足");
                    return;
                }
                #endregion
                if (GetInt("FromHomeTopMenu") == 1)
                {
                    CallBacker.Redirect(this.VirPath + "SYSN/view/produceV2/MaterialRegisters/RegisterList.ashx");
                }
                else
                {
                    CallBacker.Refresh();
                    CallBacker.CloseWindow();
                }
            }
            else
            {
                Sql.RollbackTrans();
                CallBacker.MessageBox("保存失败！");
            }
        }

        protected override void Bill_OnDelete(BillSaveDataCollection SaveDatas)
        {
            if (!CostAnalysisModule.IsCostAnalysis(BillApplyTypeEnum.B55004_生产物料登记, Bill.Ord, ""))
            {
                var kuoutids = MaterialRegHelper.Instance.GetKuoutList2sByRegBill(new int[] { Bill.Ord });
                base.Bill_OnDelete(SaveDatas);
                MaterialHelper.Instance.ReAnalysisRoomKuInfo(kuoutids);
            }
            else
            {
                CallBacker.MessageBox("该物料登记所在月份已核算成本,无法删除!");
                Sql.RollbackTrans();
                return;
            }
        }

        // 处理影响到本模块的变更
        public override List<BillChangeResultClass> Bill_AfterChange(SQLClass CurrSql, List<int> BillIds, BillApplyTypeEnum ParentBillType, DataSet ChangeDatas)
        {
            List<BillChangeResultClass> lb = new List<BillChangeResultClass>();
            foreach (DataTable dt in ChangeDatas.Tables)
            {
                if (dt.TableName == "listdata")
                {
                    CurrSql.CreateSqlTableByDataTable("#listdatatb", dt, false);
                    int result = CurrSql.Execute(@"UPDATE mrl SET mrl.intro = ld.remark FROM #listdatatb ld
                    INNER JOIN dbo.M2_MaterialRegisterLists mrl ON ld.ID = mrl.ListID
                    INNER JOIN dbo.M2_MaterialRegisters mr ON mrl.MRID = mr.ID AND mr.del = 1 AND mr.OrderType = 1
                    DROP TABLE #listdatatb");
                    if (result > 0)
                    {
                        foreach (var item in BillIds)
                        {
                            BillChangeResultClass a = new BillChangeResultClass();
                            a.BillType = BillApplyTypeEnum.B55004_生产物料登记;
                            a.BillID = item;
                            a.Result = BillChangeResultEnum.Success;
                            a.Remark = "变更成功";
                            lb.Add(a);
                        }
                    }
                }
            }
            return lb;
        }
        #endregion

    }
}
