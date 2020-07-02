using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.DataEntity;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;



namespace Kingdee.K3.STK.BillPlugIn.BillRecordLog
{
    [Kingdee.BOS.Util.HotUpdate]
    public class BillRecordLogBillPlugin : AbstractBillPlugIn
    {
        //存值变化的字段
        List<Dictionary<string, string>> colist ;
        //旧表头值
        Dictionary<string, string> oldvaluefoSubject ;
        //旧表体值
        List<Dictionary<string, string>> oldvaluesfoItem ;
        //表单名，表单标识
        string billname, billkey="";




        public override void BeforeClosed(BeforeClosedEventArgs e)
        {
            base.BeforeClosed(e);
        }
        
        public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
        {
            base.BeforeSetItemValueByNumber(e);
        }
        
        public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
        {
            base.BeforeUpdateValue(e);
        }
        

        

        /// <summary>
        /// 绑定数据后
        /// </summary>
        /// <param name="e"></param>
        public override void AfterBindData(EventArgs e)
        {
            colist = new List<Dictionary<string, string>>();
            oldvaluefoSubject = new Dictionary<string, string>();
            oldvaluesfoItem = new List<Dictionary<string, string>>();

            DynamicObject oldList = this.View.Model.DataObject as DynamicObject;
            DynamicObject st;
            DynamicObjectCollection stc;
            Dictionary<string, string> itemCol = new Dictionary<string, string>();
            billname = this.Model.BillBusinessInfo.Elements[0].Name;
            billkey = this.Model.BillBusinessInfo.Elements[0].Id;
            string[] colKeyArr = GetColKey(billname, billkey);
            foreach (var properties in oldList.DynamicObjectType.Properties)
            {
                if (colKeyArr.Contains(properties.Name) || oldList[properties] != null && oldList[properties].GetType() == typeof(DynamicObjectCollection))
                {
                    object s = oldList[properties];
                    if (s != null && s.GetType() == typeof(DynamicObject))
                    {
                        st = (DynamicObject)s;
                        string columnType = GetColKeyTypeFoCol(billname, billkey, properties.Name);

                        switch (columnType)
                        {
                            //基础资料
                            case "B":
                                oldvaluefoSubject.Add(properties.Name, st["Name"] + "");
                                break;
                            //辅助资料
                            case "C":
                                oldvaluefoSubject.Add(properties.Name, st["FDataValue"] + "");
                                break;
                            //组织
                            case "D":
                                oldvaluefoSubject.Add(properties.Name, st["Name"] + "");
                                break;
                            //用户
                            case "E":
                                break;

                        }
                    }
                    else if (s != null && s.GetType() == typeof(DynamicObjectCollection))
                    {
                        stc = (DynamicObjectCollection)s;
                        int i = 1;
                        foreach (var items in stc)
                        {
                            itemCol = new Dictionary<string, string>();
                            foreach (var col in items.DynamicObjectType.Properties)
                            {
                                if (colKeyArr.Contains(col.Name))
                                {
                                    
                                    itemCol.Add(col.Name, Convert.ToString(
                                    items[col] != null ?
                                    items[col].GetType() == typeof(DynamicObject) ?
                                    ((DynamicObject)items[col])["Name"] : items[col] : ""));
                                    
                                }
                            }
                            if (itemCol.Count > 0)
                            {
                                itemCol.Add("Row", i + "");
                                oldvaluesfoItem.Add(itemCol);
                            }
                            i++;
                        }
                    }
                    else
                    {
                        oldvaluefoSubject.Add(properties.Name, s + "");
                    }
                }
            }
            StaticClass.oldvaluefoSubject = oldvaluefoSubject;
            StaticClass.oldvaluesfoItem = oldvaluesfoItem;
        }
        

        /// <summary>
        /// 返回记录字段标识数组集合
        /// </summary>
        /// <param name="billname">表单名</param>
        /// <param name="billkey">表单标识</param>
        /// <returns></returns>
        private string[] GetColKey(string billname,string billkey)
        {
            string sql = @"/*dialect*/ select BSE.F_PAEZ_COLUMNKEY 
from PAEZ_t_BillLogColSetup bs
left join PAEZ_BillLogColSetupEntry bse on bse.FID=bs.FID
WHERE BS.F_PAEZ_BILLNAME='{0}' 
AND BS.F_PAEZ_BILLKEY='{1}'
AND BS.FDOCUMENTSTATUS='C' ";
            string sql2 = @"/*dialect*/ select BSE.F_PAEZ_ENTITYPROPERTIES 
from PAEZ_t_BillLogColSetup bs
left join PAEZ_BillLogColSetupEntry bse on bse.FID=bs.FID
WHERE BS.F_PAEZ_BILLNAME='{0}' 
AND BS.F_PAEZ_BILLKEY='{1}'
AND BS.FDOCUMENTSTATUS='C' ";
            DataSet tab = DBServiceHelper.ExecuteDataSet(Context,string.Format(sql2, billname,billkey) );
            //tab.Tables
            string[] colist = new string[tab.Tables[0].Rows.Count];
            for (int i = 0; i < colist.Length; i++)
            {
                colist[i] = tab.Tables[0].Rows[i][0] + "";
            }
            return colist;
        }

        /// <summary>
        /// 根据表单、字段名返回字段类型
        /// </summary>
        /// <param name="billname">表单名</param>
        /// <param name="billkey">表单标识</param>
        /// <param name="colkey">字段标识</param>
        /// <param name="colname">字段名</param>
        /// <returns></returns>
        private string GetColKeyTypeFoCol(string billname, string billkey, string colkey, string colname ="")
        {
            string sql = @"/*dialect*/ select F_PAEZ_COLTYPEITEM from PAEZ_t_BillLogColSetup BS
left join PAEZ_BillLogColSetupEntry BSE on bse.FID=bs.FID
WHERE BS.F_PAEZ_BILLNAME='{0}' 
AND BS.F_PAEZ_BILLKEY='{1}'
/*AND BSE.BSE.F_PAEZ_COLUMN=''*/
AND BSE.F_PAEZ_COLUMNKEY='{2}'
AND BS.FDOCUMENTSTATUS='C'";

            string sql2 = @"/*dialect*/ select F_PAEZ_COLTYPEITEM from PAEZ_t_BillLogColSetup BS
left join PAEZ_BillLogColSetupEntry BSE on bse.FID=bs.FID
WHERE BS.F_PAEZ_BILLNAME='{0}' 
AND BS.F_PAEZ_BILLKEY='{1}'
/*AND BSE.BSE.F_PAEZ_COLUMN=''*/
AND BSE.F_PAEZ_ENTITYPROPERTIES='{2}'
AND BS.FDOCUMENTSTATUS='C'";
            DataSet tab = DBServiceHelper.ExecuteDataSet(Context, string.Format(sql2, billname, billkey, colkey));
            
            return tab.Tables[0].Rows[0][0].ToString();
        }

        private string GetColKeyTypeFoKey(string bill,string colkey)
        {
            string sql = @"/*dialect*/ select F_PAEZ_COLTYPE from PAEZ_t_BillLogColSetup where F_PAEZ_BILL='{0}' and F_PAEZ_COLUMNKEY='{1}'";
            DataSet tab = DBServiceHelper.ExecuteDataSet(Context, string.Format(sql, bill, colkey));

            return tab.Tables[0].Rows[0][0].ToString();
        }

        /// <summary>
        /// 字段值改变事件
        /// </summary>
        /// <param name="e"></param>
        public override void DataChanged(DataChangedEventArgs e)
        {
            
            
            if (GetColKey(billname, billkey).Contains(e.Field.PropertyName))
            {
                string columnType = GetColKeyTypeFoCol(billname,billkey,e.Field.PropertyName);
                string sMaterialName = "";
                DynamicObject oMaterial = this.View.Model.GetValue(e.Field.Key, e.Row) as DynamicObject;
                if (oMaterial != null|| columnType=="A")
                {
                    switch (columnType)
                    {
                        //基础资料
                        case "B":
                            sMaterialName = Convert.ToString(oMaterial["Name"]);
                            break;
                        //辅助资料
                        case "C":
                            sMaterialName = Convert.ToString(oMaterial["FDataValue"]);
                            break;
                        //组织
                        case "D":
                            sMaterialName = Convert.ToString(oMaterial["Name"]);
                            break;
                        //用户
                        case "E":
                            break;
                            //默认
                        case "A":
                            sMaterialName = Convert.ToString(e.NewValue);
                            break;
                    }
                     
                }
                //过滤重复修改的字段
                foreach (var item in colist)
                {
                    if (item["Column"] == e.Field.Name && item["ColKey"] ==e.Field.Key && item["Row"] == (Convert.ToInt32(item["Row"])==0?e.Row:e.Row+1).ToString())
                    {
                        colist.Remove(item);
                        break;
                    }
                }

                //初始化为新增数据
                string oldval = "",
                    row = e.Row+1+"",
                    Operate="新增";

                //表体
                foreach (var item in oldvaluesfoItem)
                {
                    if (item.ContainsKey(e.Field.PropertyName)&& item["Row"]==e.Row+1+"")
                    {
                        if (item[e.Field.PropertyName]!= sMaterialName)
                        {
                            oldval = item[e.Field.PropertyName];
                            //item[e.Field.PropertyName]= sMaterialName;
                            row = item["Row"];
                            Operate = "修改";
                        }
                        
                    }
                }
                //主表
                if (oldvaluefoSubject.ContainsKey(e.Field.PropertyName))
                {
                    if (oldvaluefoSubject[e.Field.PropertyName]!= sMaterialName)
                    {
                        oldval = oldvaluefoSubject[e.Field.PropertyName];
                        //oldvaluefoSubject[e.Field.PropertyName] = sMaterialName;
                        row = "0";
                        Operate = "修改";
                    }
                }
                this.colist.Add(new Dictionary<string, string>() {
                    { "Column", e.Field.Name  },{ "ColKey",e.Field.Key},{ "ColEntityKey",e.Field.PropertyName},{ "OperateName",Operate},{ "Row", row },
                    { "Old", oldval }, { "New", sMaterialName }
                });
                if (Operate=="新增")
                {
                    oldvaluesfoItem.Add(new Dictionary<string, string> { { e.Field.PropertyName, sMaterialName },{ "Row", row } });
                    StaticClass.oldvaluesfoItem = oldvaluesfoItem;
                }
                
            }
        }

        

        /// <summary>
        /// 保存后触发
        /// </summary>
        /// <param name="e"></param>
        public override void AfterSave(AfterSaveEventArgs e)
        {
            string sql = @"/*dialect*/ insert into PAEZ_t_BillRecordLog
(FUSERID,FDATETIME,FORGID,FSUBSYSTEMID,FOPERATENAME,FCLIENTIP,F_PAEZ_Bill,F_PAEZ_ColumnName,F_PAEZ_OldValue,F_PAEZ_NewValue,F_PAEZ_DataRowNumber,F_PAEZ_BILLNAME,F_PAEZ_BILLKEY) 
values('{0}','{1}','{2}','','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}')";
            foreach (var item in colist)
            {
                if (item["Old"] != item["New"])
                {
                    DBServiceHelper.Execute(this.Context, string.Format(sql,
                    Context.UserName,//用户
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),//操作时间
                    Context.CurrentOrganizationInfo.Name,//组织
                    item["OperateName"],//操作类型
                    Context.ClientInfo.IpAddress,//IP
                    View.BillBusinessInfo.Elements[0].Name,//单据
                    item["Column"],//字段名
                    item["Old"],//改前
                    item["New"],//改后
                    item["Row"],//行
                    billname, //表名
                    billkey));//表标识
                }
                item["Old"] = item["New"];

                //表体
                foreach (var olditem in oldvaluesfoItem)
                {
                    if (olditem.ContainsKey(item["ColEntityKey"]) && olditem["Row"] == item["Row"])
                    {
                        if (olditem[item["ColEntityKey"]] != item["New"])
                        {
                            olditem[item["ColEntityKey"]] = item["New"];
                            StaticClass.oldvaluesfoItem = oldvaluesfoItem;
                            break;
                        }

                    }
                }
                //主表
                if (oldvaluefoSubject.ContainsKey(item["ColEntityKey"]))
                {
                    if (oldvaluefoSubject[item["ColEntityKey"]] != item["New"])
                    {
                        oldvaluefoSubject[item["ColEntityKey"]] = item["New"];
                        StaticClass.oldvaluefoSubject = oldvaluefoSubject;
                    }
                }
                
            }
            colist=colist.FindAll(item => item["Old"] != item["New"]);
        }

        /// <summary>
        /// 删除表体行时触发
        /// </summary>
        /// <param name="e"></param>
        public override void AfterDeleteRow(AfterDeleteRowEventArgs e)
        {
            Dictionary<string, string> dele = new Dictionary<string, string>();
            DynamicObject dedata = e.DataEntity;
            string col, key="";
            string[] colKeyArr = GetColKey(billname, billkey);
            foreach (var item in dedata.DynamicObjectType.Properties)
            {
                if (colKeyArr.Contains(item.Name))
                {
                    key = item.Name;
                }
            }
            col = GetColumnName(billname, billkey, key);
            foreach (var item in oldvaluesfoItem)
            {
                if (item.ContainsKey(key)&& item["Row"] == e.Row + 1 + "")
                {
                    dele = item;
                    oldvaluesfoItem.Remove(item);
                    foreach (var item2 in oldvaluesfoItem)
                    {
                        if (e.Row + 1 < Convert.ToInt32(item2["Row"]))
                        {
                            item2["Row"] = (Convert.ToInt32(item2["Row"]) - 1) + "";
                        }
                    }
                    break;
                }
            }
            StaticClass.oldvaluesfoItem = oldvaluesfoItem;
            bool isAdd = true;
            foreach (var item in colist)
            {
                if (item["Column"] == col && item["ColEntityKey"]== key&& item["Row"]== e.Row + 1 + ""&& item["OperateName"]=="新增")
                {
                    colist.Remove(item);
                    isAdd = false;
                    break;
                }
            }
            if (isAdd)
            {
                colist.Add(new Dictionary<string, string>() {
                    { "Column", col  },{ "ColKey",key},{"ColEntityKey",key },{ "OperateName","删除"},{ "Row", e.Row+1+"" },
                    { "Old", dele[key] }, { "New", "" }
                });
            }
            


        }

        /// <summary>
        /// 获取字段名
        /// </summary>
        /// <param name="billName"></param>
        /// <param name="billKey"></param>
        /// <param name="colKey">字段标识</param>
        /// <returns></returns>
        private string GetColumnName(string billName,string billKey,string colKey)
        {
            string sql = @"/*dialect*/   select BSE.F_PAEZ_COLUMN
  from PAEZ_t_BillLogColSetup bs
  left join PAEZ_BillLogColSetupEntry bse on bse.FID=bs.FID
  WHERE BS.F_PAEZ_BILLNAME='{0}' 
  AND BS.F_PAEZ_BILLKEY='{1}'
  AND BSE.F_PAEZ_ENTITYPROPERTIES='{2}'
  AND BS.FDOCUMENTSTATUS='C'";
            DataSet tab = DBServiceHelper.ExecuteDataSet(Context, string.Format(sql, billName, billKey,colKey));
            return tab.Tables[0].Rows[0][0].ToString();
        }
        
    }
}
