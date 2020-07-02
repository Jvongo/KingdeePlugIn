using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.K3.STK.BillPlugIn.BillRecordLog
{
    [Kingdee.BOS.Util.HotUpdate]
    public class BillRecordLogBillPlugin: AbstractBillPlugIn
    {
        List<Dictionary<string,string>> colist=new List<Dictionary<string, string>>();
        //Dictionary<string, string> oldvalues = new Dictionary<string, string>();
        string billstate;
        DynamicObject oldList;


        public override void PreOpenForm(PreOpenFormEventArgs e)
        {
            //Kingdee.BOS.Core.Metadata.OperationStatus.ADDNEW
            //e.OpenParameter.InitStatus;
            //this.Model.OpenParameter.InitStatus
        }

       

        public override void AfterBindData(EventArgs e)
        {
            List<Element> conList = this.Model.BillBusinessInfo.Elements;
            //DynamicObject conList = this.Model.DataObject;
            //DynamicObject control;
            foreach (var conItem in conList)
            {
                //control = this.Model.GetValue(conItem.Key) as DynamicObject;
                //oldvalues.Add(conItem.Name,)
            }
        }

        public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
        {
            oldList = this.Model.DataObject;
            DynamicObject oMaterial = this.View.Model.GetValue("FMaterialId") as Kingdee.BOS.Orm.DataEntity.DynamicObject;
            foreach (var properties in oMaterial.DynamicObjectType.Properties)
            {
                object s = oMaterial[properties];
            }
        }

        public override void DataChanged(DataChangedEventArgs e)
        {
            //DynamicObject oMaterial = this.View.Model.GetValue("F_PAEZ_Base", e.Row) as DynamicObject;
            DynamicObject oMaterial = this.View.Model.GetValue(e.Field.Key, e.Row) as DynamicObject;
            if (oMaterial != null)
            {
                string sMaterialName = Convert.ToString(oMaterial["Name"]);
            }

            DynamicObject oOldBase = e.OldValue as DynamicObject;
            string s=this.Model.GetValue("FBillNo")+"";
            string colType,tabNAme;
            string oldValue, newValue;
            
            if (IsKeyColumn(e.Field+"", this.View.BillBusinessInfo.Elements[0].Name,out colType,out tabNAme))
            {
                
                foreach (var item in colist)
                {
                    if (item["Column"] == e.Field + "" && item["Row"] == e.Row + "")
                    {
                        colist.Remove(item);
                        break;
                    }
                }

                switch (colType)
                {
                    case "基础资料":
                        IsTypeJCZL(e, tabNAme, out oldValue,out newValue);
                        break;
                    case "辅助资料":
                        IsTypeFZZL(e, tabNAme, out oldValue, out newValue);
                        break;
                    case "组织":
                        IsTypeZZ(e, tabNAme, out oldValue, out newValue);
                        break;
                    case "用户":
                        IsTypeYH(e, tabNAme, out oldValue, out newValue);
                        break;
                    case "默认":
                        break;
                }

                this.colist.Add(new Dictionary<string, string>() { { "Column", e.Field + "" },{ "ColType",colType}, { "Row", e.Row + "" }, { "Old", e.OldValue + "" }, { "New", e.NewValue + "" } });
            }
            //DBServiceHelper.Execute(this.Context, "");
        }

        /// <summary>
        /// 判断是否是记录字段，并返回字段类型
        /// </summary>
        /// <param name="colName">字段名</param>
        /// <param name="billNAme">表单名</param>
        /// <param name="colType">字段类型</param>
        /// <param name="tabName">表名</param>
        /// <returns></returns>
        private bool IsKeyColumn(string colName,string billNAme,out string colType,out string tabName)
        {
            string sql = @"/*dialect*/ select F_PAEZ_COLTYPE,F_PAEZ_TABLENAME from PAEZ_t_BillLogColSetup where F_PAEZ_BILL='{0}' and F_PAEZ_COLUMN='{1}'";
            DataSet tab=DBServiceHelper.ExecuteDataSet(this.Context, string.Format(sql, billNAme, colName));
            colType = tab.Tables[0].Rows.Count==0?"": tab.Tables[0].Rows[0][0]+"";
            tabName= tab.Tables[0].Rows.Count == 0 ? "" : tab.Tables[0].Rows[0][1] + "";
            if (colType!=""&&tabName!="")
                return true;
            return false;
        }
        private void IsTypeJCZL(DataChangedEventArgs e,string tabName, out string oldValue, out string newValue)
        {
            oldValue = ""; 
            newValue = "";
            
        }
        private void IsTypeFZZL(DataChangedEventArgs e, string tabName, out string oldValue, out string newValue)
        {
            oldValue = "";
            newValue = "";
        }
        private void IsTypeZZ(DataChangedEventArgs e, string tabName, out string oldValue, out string newValue)
        {
            oldValue = "";
            newValue = "";
        }
        private void IsTypeYH(DataChangedEventArgs e, string tabName, out string oldValue, out string newValue)
        {
            oldValue = "";
            newValue = "";
        }

        /// <summary>
        /// 保存后,写入日志操作
        /// </summary>
        /// <param name="e"></param>
        public override void AfterSave(AfterSaveEventArgs e)
        {
            //List<SqlParam> lsp;
            //string sql = @"/*dialect*/ insert into PAEZ_t_BillRecordLog
            //(FUSERID,FDATETIME,FORGID,FSUBSYSTEMID,FOPERATENAME,FCLIENTIP,F_PAEZ_Bill,F_PAEZ_ColumnName,F_PAEZ_OldValue,F_PAEZ_NewValue,F_PAEZ_DataRowNumber) 
            //values(@UserName,@time,@forgid,'','',@IP,@Bill,@ColumnName,@OldValue,@NewValue,@DataRowNumber)";
            //foreach (var item in colist)
            //{
            //    lsp = new List<SqlParam>() {
            //                    new SqlParam("@UserName",KDDbType.String,this.Context.UserName),
            //                    new SqlParam("@time",KDDbType.DateTime,System.DateTime.Now),
            //                    new SqlParam("@forgid",KDDbType.String,this.Context.CurrentOrganizationInfo.Name),
            //                    new SqlParam("@IP",KDDbType.String,this.Context.ClientInfo.IpAddress),
            //                    new SqlParam("@Bill",KDDbType.String,this.View.BillBusinessInfo.Elements[0].Name),
            //                    new SqlParam("@ColumnName",KDDbType.String,item["Column"]),
            //                    new SqlParam("@OldValue",KDDbType.String,item["Old"]),
            //                    new SqlParam("@NewValue",KDDbType.String,item["New"]),
            //                    new SqlParam("@DataRowNumber",KDDbType.String,item["Row"]),
            //                };
            //    DBServiceHelper.Execute(this.Context, sql, lsp);
            //}


            string sql = @"/*dialect*/ insert into PAEZ_t_BillRecordLog
(FUSERID,FDATETIME,FORGID,FSUBSYSTEMID,FOPERATENAME,FCLIENTIP,F_PAEZ_Bill,F_PAEZ_ColumnName,F_PAEZ_OldValue,F_PAEZ_NewValue,F_PAEZ_DataRowNumber) 
values('{0}','{1}','{2}','','','{3}','{4}','{5}','{6}','{7}','{8}')";
            foreach (var item in colist)
            {
                DBServiceHelper.Execute(this.Context,string.Format(sql, Context.UserName, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Context.CurrentOrganizationInfo.Name
                    , Context.ClientInfo.IpAddress, View.BillBusinessInfo.Elements[0].Name, item["Column"], item["Old"], item["New"], item["Row"]));
            }
            colist.Clear();
            
        }
        /// <summary>
        /// 保存前
        /// </summary>
        /// <param name="e"></param>
        public override void BeforeSave(BeforeSaveEventArgs e)
        {
            base.BeforeSave(e);
        }
        public override void BeforeDeleteRow(BeforeDeleteRowEventArgs e)
        {
            base.BeforeDeleteRow(e);
        }
        public override void BeforeDeleteEntry(BeforeDeleteEntryEventArgs e)
        {
            base.BeforeDeleteEntry(e);
        }
    }
}
