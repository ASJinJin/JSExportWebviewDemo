using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using ZBServices.db.Tables.Produce;
using ZBServices.flib.Produce.CostAnalysis;
using ZBServices.sdk.bill;
using ZBServices.sdk.listview;
using ZBServices.sdk.orm;
using ZBServices.ui;
using ZBServices.view.SYSN.mdl.produceV2;

namespace ZBServices.view.SYSN.view.produceV2.MaterialRegisters
{
    /// <summary>
    /// 物料登记列表
    /// </summary>
    public class registerList : ReportPage<RegistersAdd>
    {
        // 物料登记列表
        // 物料登记列表
        // 物料登记列表
        // 物料登记列表
        // 物料登记列表
        // 物料登记列表
        public string name;
        public string name{ get{return name;}}
        
        protected override void OnBillReportInit(ReportInitEventType callType)
        {
            Report.Title = "物料登记列表";
            #region 顶部按钮
            Report.CommandButtons.Add(BillCommCmdButtonEnum.ExportButton);
            Report.CommandButtons.Add(BillCommCmdButtonEnum.PrintButton);
            
            /*sdgfjkdlsfja
             adsfgas
             asgdas
             agsdasd
             adsgsag
             agagads*/
            
            if (SystemPowerClass.ExistsPower(SQLPowerTypeEnum.物料登记, 13))
            {
                Report.CommandButtons.Add("登记", "Addpage").OpenURL = "" + this.VirPath + "SYSN/view/produceV2/MaterialRegisters/RegistersAdd.ashx?fromType=" + GetInt("fromType", 1);
                Report.CommandButtons.Add("批量登记", "Addpage").OpenURL = "" + this.VirPath + "SYSN/view/produceV2/MaterialRegisters/BatchRegisters.ashx?fromType=" + GetInt("fromType", 1);
            }
            #endregion

            #region 列表基础定义
            ListViewClass lvw = Report.Listview;
            lvw.Page.PageSize = 10;
            string mainSQL = "EXEC [dbo].[erp_MaterialRegisterLists] " + SessionInfoClass.CurrUserID + ",@pageindex,@pagesize,@date1_0,@date1_1,@inDate_0,@inDate_1,@hszt,@serchkey,@serchkeyTxt,@Creator,@cateidsp,@lastsp,@title,@MOBH,@cptitle,@cporder,@cptype,@childname,@childorder,@childtype,@spzt,'{@orderby}',@ApproveStatus,@RelationInKu,@ExportSign";
            lvw.Source.MainSql = mainSQL;
            lvw.UI.CheckBox = true;
            lvw.UI.CheckBoxDBName = "ID";
            if (SystemPowerClass.ExistsPower(SQLPowerTypeEnum.物料登记, 3))
            {
                Report.BatchButtons.Add("批量删除", "batchdel");
            }
            #endregion 列表基础定义

            #region 表头定义
            ListViewClass.ListViewHeaderCollection listColumns = lvw.Headers;
            ListViewHeader column;
            //列表
            column = listColumns.Add("登记主题", "title").CAlign("left");
            column.FormatHtml = "script:app.CLinkHtml('@title', '" + this.VirPath + "SYSN/view/produceV2/MaterialRegisters/RegistersAdd.ashx?ord=@ID&view=details', 1, @pdetail)";
            listColumns.Add("登记编号", "MOBH");
            listColumns.Add("父件产品", "protitle").CAlign("left");
            listColumns.Add("核算状态", "hszt").Visible = SystemPowerClass.ExistsModule(18500);
            listColumns.Add("审批状态", "spstatus");
            listColumns.Add("添加人员", "name");
            listColumns.Add("登记日期", "date1");
            listColumns.Add("操作", "operte").CWidth(200).FormatHtml = "button:[详情,@pdetail][审批][改批][修改][删除]";
            SetBillListSignCol("", "spstatus");
            #endregion

            #region  排序设置
            Report.OrderFields.Add("inDate", "添加时间");
            Report.OrderFields.Add("title", "登记主题");
            Report.OrderFields.Add("MOBH", "登记编号");
            Report.OrderFields.Add("hszt", "核算状态");
            Report.OrderFields.Add("date1", "登记日期");
            Report.OrderFields.Add("sptime", "审批时间");
            Report.OrderFields.Add("Creator", "添加人员");
            Report.OrderFieldsSetDefault("-inDate");
            #endregion

            #region 快捷检索
            BillFieldCollection kj = Report.Searchs;
			var maxdt = M2_MaterialRegisters.Group_By(x => new { Date1 = DBQuery.Max(x.Date1) }).Select(x => new { x.Date1 }).ToList();
			var rangemax = ((maxdt.Count == 0 || maxdt.FirstOrDefault().Date1 < DateTime.Now)
				? DateTime.Now
                : maxdt.FirstOrDefault().Date1).ToString("yyyy-MM-dd");
			BillRangeBoxField inDate = kj.Add<BillRangeBoxField>("登记日期", "date1", ConvertHelper.ToDateStr(DateTime.Now.AddYears(-1).AddDays(1)) + "," + rangemax);
            inDate.UiSkin = BillRangeFieldUITypeEnum.Date;
            DataSourceOptions sclxop = new DataSourceOptions();
            sclxop.Options.Add("核算状态", "");
            sclxop.Options.Add("未核算", "0");
            sclxop.Options.Add("已核算", "1");
            BillSelectBoxField hszt = kj.AddSelectBox("", "hszt");
            hszt.Visible = SystemPowerClass.ExistsModule(18500);
            hszt.Source = sclxop;
            DataSourceOptions pname = new DataSourceOptions();
            pname.Options.Add("登记主题", "title");
            pname.Options.Add("登记编号", "MOBH");
            pname.Options.Add("父件名称", "ptitle");
            pname.Options.Add("物料名称", "protitle");
            pname.Options.Add("物料编号", "proorder1");
            pname.Options.Add("物料型号", "protype1");
            pname.Options.Add("当前审批人", "cateidsp");
            pname.Options.Add("最后审批人", "lastsp");
            kj.AddSelectBox("", "serchkey").Source = pname;
            kj.AddText("", "serchkeyTxt");
            #endregion

            #region 高级检索
            BillFieldCollection adSearch = Report.AdSearchs;
            BillLinkBoxField Creator = adSearch.AddLinkBox("添加人员", "Creator");
            Creator.CAutoComplete(CommAutoComplete.dlg_账号列表_检索, true);
            BillLinkBoxField cateidsp = adSearch.AddLinkBox("当前审批人", "cateidsp");
            cateidsp.CAutoComplete(CommAutoComplete.dlg_账号列表_检索, true);
            BillLinkBoxField lastsp = adSearch.AddLinkBox("最后审批人", "lastsp");
            lastsp.CAutoComplete(CommAutoComplete.dlg_账号列表_检索, true);
            adSearch.AddText("登记主题", "title");
            adSearch.AddText("登记编号", "MOBH");
            adSearch.AddText("父件名称", "cptitle");
            adSearch.AddText("父件编号", "cporder");
            adSearch.AddText("父件型号", "cptype");
            adSearch.AddText("物料名称", "childname");
            adSearch.AddText("物料编号", "childorder");
            adSearch.AddText("物料型号", "childtype");
            adSearch.AddCheckBoxs("审批状态", "spzt", "").Source = GetApproveStatusDataSource();
            BillCheckBoxsField hszts = adSearch.AddCheckBoxs("核算状态", "hszt", "");
            hszts.SetOptions("未核算=0,已核算=1");
            hszts.Visible = SystemPowerClass.ExistsModule(18500);

            BillCheckBoxsField relationInKu = adSearch.AddCheckBoxs("关联入库", "RelationInKu", "");
            relationInKu.SetOptions("未关联入库=0,已关联=1");

            BillRangeBoxField date1 = adSearch.Add<BillRangeBoxField>("登记日期", "date1");
            date1.UiSkin = BillRangeFieldUITypeEnum.Date;
            BillRangeBoxField cateDate = adSearch.Add<BillRangeBoxField>("添加时间", "inDate");
            cateDate.UiSkin = BillRangeFieldUITypeEnum.DateTime;
            #endregion
        }
		private DateTime currCostDate = DateTime.Parse("1912-1-1");

		//当前行数据是否已经成本分析
		private bool IsCostAnalysis(DataRow row) {
			if (currCostDate.Year == 1912) { currCostDate = CostAnalysisHelper.GetCurrDate(); }
			return DateTime.Parse(row["date1"].ToString()) < currCostDate;
		}


		#region 权限判断
		public override string PowerForCandelete(DataRow row)
        {
            if (IsCostAnalysis(row))  {  return "0"; }  //成本核算了就不能做动作了
            return base.PowerForCandelete(row);
        }

		public override string powerForCanModify(DataRow row)
		{
            int status = ConvertHelper.ToInt(row["spstatus"].ToString());
            if (IsCostAnalysis(row)|| status==0) { return "0"; }
			return base.PowerForCanModify(row);
		}

		public override string PowerForCanModifyApprove(DataRow row)
		{
            int status = ConvertHelper.ToInt(row["spstatus"].ToString());
            if (IsCostAnalysis(row)|| status == 0) { return "0"; }
			return base.PowerForCanModifyApprove(row);
		}

		public override string PowerForCanUpdateApprove(DataRow row)
		{
            int status = ConvertHelper.ToInt(row["spstatus"].ToString());
            if (IsCostAnalysis(row)||status==0) { return "0"; }
			return base.PowerForCanUpdateApprove(row);
		}

		#endregion

		protected override void App_OnReportBatchHandle(sdk.report.ReportBatchHandleClass Batchs)
        {
            var  selectIds = string.Join(",", Batchs.SelectedDatas).ToIntList();
			if (selectIds.Length == 0) { return;  }
			var dats = M2_MaterialRegisters.Where(x => x.ID.In(selectIds)).Select(x => new { x.Date1, x.ID }).GetDataTable();
            switch (Batchs.BatchCommandKey)
            {
                case "batchdel":
                    {
                        foreach (DataRow item in dats.Rows)
                        {
                            if (IsCostAnalysis(item))
                            {
                                Batchs.AddMessage(item["ID"].ToString(), "已成本核算，无法删除");
                            }
                        }
                        Batchs.RemoveSelectedDatas(Batchs.Messages.Select(v => v.n).ToList());
                    }
                    break;
            }
            base.App_OnReportBatchHandle(Batchs);
        }

        //列表按钮点击事件
        protected override void App_OnReportRowButtonClick(sdk.report.ReportRowButtonHandleClasss callback)
        {
            int ID = ConvertHelper.ToInt(callback.CurrRowData["ID"]);
            switch (callback.ButtonText)
            {
                case "详情":
                    callback.AddScript("app.OpenUrl('" + this.VirPath + "SYSN/view/produceV2/MaterialRegisters/RegistersAdd.ashx?ord=" + ID + "&view=details','MPSD')");
                    break;
                case "修改":
                    callback.AddScript("app.OpenUrl('" + this.VirPath + "SYSN/view/produceV2/MaterialRegisters/RegistersAdd.ashx?ord=" + ID + "','MPSA')");
                    break;
                default:
                    base.App_OnReportRowButtonClick(callback);
                    break;
            }
        }

        protected override void DeleteBillItem(string ords)
        {
			
            var kuoutlist2 = ZBServices.flib.Produce.Material.MaterialRegHelper.Instance.GetKuoutList2sByRegBill(ords.ToIntList());
            if (kuoutlist2 != null && kuoutlist2.Count() > 0)
            {
                base.DeleteBillItem(ords);
                ZBServices.flib.Produce.Material.MaterialHelper.Instance.ReAnalysisRoomKuInfo(kuoutlist2);
            }

        }
     
        protected override bool App_OnListViewRefreshBefore(ListViewCallBackPageSetProcAttr lvwCallBack)
        {
            if (lvwCallBack.IsExcelModel)
            {
                var lvw = lvwCallBack.ListViewObj;
                lvw.Headers.Clear();
                lvw.Source.LoadAtOnce = true;
                lvw.Source.MainSql = lvwCallBack.CurrMainSql;
                lvw.Headers["ID"].Display = BFDisplayEnum.Hidden;
                lvw.Headers["listid"].Display = BFDisplayEnum.Hidden;
                lvw.Headers["sort"].Display = BFDisplayEnum.Hidden;
                lvw.Headers["inx"].Display = BFDisplayEnum.Hidden;
                lvw.Headers["title"].CTitle("登记主题").CAlign("left");
                lvw.Headers["MOBH"].CTitle("登记编号");
                lvw.Headers["date1"].CTitle("登记日期");
                var billname = lvw.Headers["billname"].CTitle("关联单据").CAlign("left");
                var KuInTitle = lvw.Headers["KuInTitle"].CTitle("入库主题").CAlign("left");
                lvw.Headers["ptitle"].CTitle("父件名称").CAlign("left");
                var porder = lvw.Headers["porder"].CTitle("父件编号");
                var ptype = lvw.Headers["ptype"].CTitle("父件型号");
                var ParentNum = lvw.Headers["ParentNum"].CTitle("父件数量").CUIType(FieldUITypeEnum.NumberBox).CDisplay(BFDisplayEnum.ReadOnly);
                lvw.Headers["spstatus"].CTitle("审批状态");
                lvw.Headers["hszt"].CTitle("核算状态").Visible = SystemPowerClass.ExistsModule(18500);
                lvw.Headers["creator"].CTitle("添加人员");
                lvw.Headers["indate"].CTitle("添加时间");
                lvw.Headers["ctitle"].CTitle("物料名称").CAlign("left");
                lvw.Headers["corder"].CTitle("物料编号");
                lvw.Headers["ctype"].CTitle("物料型号");
                lvw.Headers["unit"].CTitle("单位");
                lvw.Headers["num1"].CTitle("物料数量");
                lvw.Headers["CostPrice"].CTitle("成本单价");
                lvw.Headers["CostPriceSum"].CTitle("成本总价");
                lvw.Headers["Ph"].CTitle("批号");
                lvw.Headers["Xlh"].CTitle("序列号");
                lvw.Headers["scrq"].CTitle("生产日期");
                lvw.Headers["yxrq"].CTitle("有效日期");
                lvw.Headers["intro"].CTitle("备注").CAlign("left");
                lvw.UI.MainCusFieldInsertPos = "物料名称";
                lvw.UI.ExportValidExpression = "title<>''";
                lvw.UI.ExportListIdDBName = "listid";
                return true;
            }
            else
            {
                foreach (var item in BillIds)
               {
                   BillChangeResultClass a = new BillChangeResultClass();
                   a.BillType = BillApplyTypeEnum.B55004_生产物料登记;
                   a.BillID = item;
                   a.Result = BillChangeResultEnum.Success;
                   a.Remark = "变更成功";
                   a.GetTable();
                   lb.Add(a);
               }
               
                return false;
            }
        }

        protected override void ConfigForBillList(IBillGlobalInformation ibill)
        {
            base.ConfigForBillList(ibill);
            Report.CommandButtons.Remove("addatreportbutton");
        }
    }
}
