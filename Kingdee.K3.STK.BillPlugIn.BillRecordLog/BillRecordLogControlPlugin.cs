using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
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
    public class BillRecordLogControlPlugin : AbstractOperationServicePlugIn
    {
        //存值变化的字段
        List<Dictionary<string, string>> colist;
        //旧表体值
        List<Dictionary<string, string>> oldvaluesfoItem;
        //新值
        List<Dictionary<string, string>> newvaluesfoItem;
        //操作类型
        string operate,billkey;


        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            //string s=this.BusinessInfo.GetForm().Id;
            List<string[]> colKey = GetColKeyToArray(BusinessInfo.GetForm().Id, "F_PAEZ_ENTITYPROPERTIES");
            // 如下代码行，指定字段xxxxx的Key，强制要求加载字段
            for (int i = 0; i < colKey[0].Length; i++)
            {
                e.FieldKeys.Add(colKey[0][i]);
            }
            
        }

        public override void OnPrepareOperationServiceOption(OnPrepareOperationServiceEventArgs e)
        {
            billkey = this.BusinessInfo.GetForm().Id;
            operate = this.FormOperation.OperationName;//操作类型
        }

        public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
        {
            oldvaluesfoItem = new List<Dictionary<string, string>>();
            //newvaluesfoItem = new List<Dictionary<string, string>>();
            string[] arr = new string[e.SelectedRows.Count()];
            //DynamicObject[] newData = new DynamicObject[e.SelectedRows.Count()];
            for (int i = 0; i < arr.Length; i++)
            {
                //获取新值的DataEntity
                //newData[i] = e.SelectedRows.ElementAt(i).DataEntity;
                arr[i] = e.SelectedRows.ElementAt(i).DataEntity["id"].ToString();
            }
            DynamicObject[] oldData = BusinessDataServiceHelper.Load(this.Context, arr, BusinessInfo.GetDynamicObjectType());
            GetOldOrNewDataList(oldData, oldvaluesfoItem);


        }

        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            colist = new List<Dictionary<string, string>>();
            newvaluesfoItem = new List<Dictionary<string, string>>();
            GetOldOrNewDataList(e.DataEntitys, newvaluesfoItem);
            Dictionary<string, string> colname = new Dictionary<string, string>();
            List<string[]> colKeyValues = GetColKeyToArray(BusinessInfo.GetForm().Id, "F_PAEZ_ENTITYPROPERTIES", "F_PAEZ_COLUMN");//key/val
            //字段名和字段实体属性组成字典
            for (int i = 0; i < colKeyValues[0].Length; i++)
                colname.Add(colKeyValues[0][i], colKeyValues[1][i]);

            //修改
            foreach (var olditem in oldvaluesfoItem)
            {
                foreach (var newitem in newvaluesfoItem)
                {
                    //修改
                    if (olditem["Number"] == newitem["Number"] && olditem["BillROM"] == newitem["BillROM"] && olditem["Id"] == newitem["Id"])
                    {
                        
                        foreach (var oitem in olditem)
                        {
                            foreach (var nitem in newitem)
                            {
                                if (nitem.Key!= "Number" && nitem.Key != "BillROM" && nitem.Key != "Id" && nitem.Key != "Row" 
                                    && oitem.Key != "Number" && oitem.Key != "BillROM" && oitem.Key != "Id" && oitem.Key != "Row")
                                {
                                    if (oitem.Key == nitem.Key && oitem.Value != nitem.Value)
                                    {
                                        colist.Add(new Dictionary<string, string>() {
                                            { "BillNo",newitem["Number"]},
                                            { "Column",colname[nitem.Key]},
                                            { "ColKey",nitem.Key},
                                            { "ColEntityKey","e.Field.PropertyName"},
                                            { "OperateName","修改"},
                                            { "Row", newitem["Row"] },
                                            { "Old", oitem.Value },
                                            { "New", nitem.Value }
                                        });
                                    }
                                }
                                
                            }
                        }
                    }
                }
            }

            //删除
            foreach (var olditem in oldvaluesfoItem)
            {
                //var a = newvaluesfoItem.Select(n => n["Number"]);
                if (newvaluesfoItem.Select(n=>n["Number"]).Contains(olditem["Number"]) 
                    && newvaluesfoItem.Select(n => n["BillROM"]).Contains(olditem["BillROM"]) 
                    && !newvaluesfoItem.Select(n => n["Id"]).Contains(olditem["Id"]))
                {
                    foreach (var oitem in olditem)
                    {
                        if (oitem.Key != "Number" && oitem.Key != "BillROM" && oitem.Key != "Id" && oitem.Key != "Row")
                        {
                            colist.Add(new Dictionary<string, string>() {
                                { "BillNo",olditem["Number"]},
                                { "Column",colname[oitem.Key]},
                                { "ColKey",oitem.Key},
                                { "ColEntityKey","e.Field.PropertyName"},
                                { "OperateName","删除"},
                                { "Row", olditem["Row"] },
                                { "Old", oitem.Value },
                                { "New", "" }
                            });
                        }
                    }
                }
                
            }

            //新增
            foreach (var newitem in newvaluesfoItem)
            {
                //var a = newvaluesfoItem.Select(n => n["Number"]);
                if (oldvaluesfoItem.Select(n => n["Number"]).Contains(newitem["Number"])
                    && oldvaluesfoItem.Select(n => n["BillROM"]).Contains(newitem["BillROM"])
                    && !oldvaluesfoItem.Select(n => n["Id"]).Contains(newitem["Id"]))
                {
                    foreach (var nitem in newitem)
                    {
                        if (nitem.Key != "Number" && nitem.Key != "BillROM" && nitem.Key != "Id" && nitem.Key != "Row")
                        {
                            colist.Add(new Dictionary<string, string>() {
                                { "BillNo",newitem["Number"]},
                                { "Column",colname[nitem.Key]},
                                { "ColKey",nitem.Key},
                                { "ColEntityKey","e.Field.PropertyName"},
                                { "OperateName","新增"},
                                { "Row", newitem["Row"] },
                                { "Old", "" },
                                { "New", nitem.Value }
                            });
                        }
                    }
                }
                else if(oldvaluesfoItem.Select(n => n["Number"]).Count()==0)
                {
                    //List<string[]> billnocol= GetColKeyToArray(billkey);
                    //string number_ = e.DataEntitys[0][billnocol[0][0]].ToString();
                    foreach (var nitem in newitem)
                    {
                        if (nitem.Key != "Number" && nitem.Key != "BillROM" && nitem.Key != "Id" && nitem.Key != "Row" && nitem.Value!="")
                        {
                            colist.Add(new Dictionary<string, string>() {
                                { "BillNo",newitem["Number"]},
                                { "Column",colname[nitem.Key]},
                                { "ColKey",nitem.Key},
                                { "ColEntityKey","e.Field.PropertyName"},
                                { "OperateName","新增"},
                                { "Row", newitem["Row"] },
                                { "Old", "" },
                                { "New", nitem.Value }
                            });
                        }
                    }
                }

            }

            //用户\操作时间\组织\子系统\操作类型\IP\单据\字段名\改前\改后\第几行\表名\表标识\单据编号
            string sql = @"/*dialect*/ insert into PAEZ_t_BillRecordLog
(FUSERID,FDATETIME,FORGID,FSUBSYSTEMID,FOPERATENAME,FCLIENTIP,F_PAEZ_Bill,F_PAEZ_ColumnName,F_PAEZ_OldValue,F_PAEZ_NewValue,F_PAEZ_DataRowNumber,F_PAEZ_BILLNAME,F_PAEZ_BILLKEY,F_PAEZ_BILLNO) 
values('{0}','{1}','{2}','','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}')";

            foreach (var item in colist)
            {
                DBServiceHelper.Execute(Context, string.Format(sql,
                    Context.UserName,//用户
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),//操作时间
                    Context.CurrentOrganizationInfo.Name,//组织
                    item["OperateName"],//操作类型
                    Context.ClientInfo.IpAddress,//IP
                    BusinessInfo.Elements[0].Name,//单据
                    item["Column"],//字段名
                    item["Old"],//改前
                    item["New"],//改后
                    item["Row"],//行
                    BusinessInfo.Elements[0].Name, //表名
                    BusinessInfo.Elements[0].Id,
                    item["BillNo"]
                    ));
            }

            
        }


        /// <summary>
        /// 获取日志记录设置表设置的字段值
        /// </summary>
        /// <param name="dyDataList"></param>
        private void GetOldOrNewDataList(DynamicObject[] dyDataList, List<Dictionary<string, string>> valuesfoItem)
        {
            List<string[]> colkeyArr = GetColKeyToArray(billkey, "F_PAEZ_PARENTORM", "F_PAEZ_ENTITYPROPERTIES", "F_PAEZ_COLTYPEITEM");
            Dictionary<string, string> collog, collog_p;

            Dictionary<string, string> coltype = new Dictionary<string, string>();
            for (int i = 0; i < colkeyArr[1].Length; i++)
                coltype.Add(colkeyArr[1][i], colkeyArr[2][i]);


            //添加记录字段值
            foreach (var dyData in dyDataList)
            {
                collog_p = new Dictionary<string, string>();
                collog_p.Add("Number", dyData[colkeyArr[3][0]] == null ? "" : dyData[colkeyArr[3][0]].ToString());
                collog_p.Add("BillROM", dyData.DynamicObjectType.Name);
                collog_p.Add("Row", "1");
                collog_p.Add("Id", dyData["Id"].ToString());

                foreach (var properties in dyData.DynamicObjectType.Properties)
                {
                    int row = 1;
                    //表单里的页签
                    if (colkeyArr[0].Contains(properties.Name))
                    {
                        foreach (var collist in (DynamicObjectCollection)dyData[properties])
                        {
                            collog = new Dictionary<string, string>();
                            collog.Add("Number", dyData[colkeyArr[3][0]] == null ? "" : dyData[colkeyArr[3][0]].ToString());
                            collog.Add("BillROM", properties.Name);
                            collog.Add("Row", row.ToString());
                            collog.Add("Id", collist["Id"].ToString());
                            row++;
                            foreach (var colitem in collist.DynamicObjectType.Properties)
                            {
                                //页签里的字段
                                if (colkeyArr[1].Contains(colitem.Name))
                                {
                                    switch (coltype[colitem.Name])
                                    {
                                        //默认
                                        case "A":
                                            collog.Add(colitem.Name, collist[colitem.Name] == null ? "" : collist[colitem.Name].ToString());
                                            break;
                                        //基础资料
                                        case "B":
                                            collog.Add(colitem.Name,
                                                collist[colitem.Name] == null ? "" : (collist[colitem.Name].GetType() == typeof(DynamicObject) ? ((DynamicObject)collist[colitem.Name])["Name"] : collist[colitem.Name]).ToString().Trim());//判断 是否为null值，是否是int值，是否是DynamicObject类型  null值取""  int取本身  DynamicObject取["Name"]
                                            break;
                                        //辅助资料
                                        case "C":
                                            collog.Add(colitem.Name, ((DynamicObject)collist[colitem.Name])["FDataValue"].ToString().Trim());
                                            break;
                                        //组织
                                        case "D":
                                            collog.Add(colitem.Name,
                                                collist[colitem.Name] == null ? "" : ((DynamicObject)collist[colitem.Name])["Name"].ToString().Trim());//判断 是否为null值，是否是int值，是否是DynamicObject类型  null值取""  int取本身  DynamicObject取["Name"]
                                            break;
                                        //用户
                                        case "E":
                                            collog.Add(colitem.Name,
                                                collist[colitem.Name] == null ? "" : (collist[colitem.Name].GetType() == typeof(DynamicObject) ? ((DynamicObject)collist[colitem.Name])["Name"] : collist[colitem.Name]).ToString().Trim());//判断 是否为null值，是否是int值，是否是DynamicObject类型  null值取""  int取本身  DynamicObject取["Name"]
                                            break;
                                        //下拉列表
                                        case "F":
                                            collog.Add(colitem.Name, collist[colitem.Name] == null ? "" : collist[colitem.Name].ToString());
                                            break;

                                    }

                                }
                            }
                            valuesfoItem.Add(collog);
                        }
                    }
                    if (colkeyArr[1].Contains(properties.Name))
                    {
                        switch (coltype[properties.Name])
                        {
                            //默认
                            case "A":
                                collog_p.Add(properties.Name, dyData[properties.Name] == null ? "" : dyData[properties.Name].ToString());
                                break;
                            //基础资料
                            case "B":
                                collog_p.Add(properties.Name,
                                    dyData[properties.Name] == null ? "" : ((DynamicObject)dyData[properties.Name])["Name"].ToString().Trim());//系统值更新时间空值可能是一个空格
                                break;
                            //辅助资料
                            case "C":
                                collog_p.Add(properties.Name,
                                    dyData[properties.Name] == null ? "" : ((DynamicObject)dyData[properties.Name])["FDataValue"].ToString().Trim());//系统值更新时间空值可能是一个空格
                                break;
                            //组织
                            case "D":
                                collog_p.Add(properties.Name,
                                    dyData[properties.Name] == null ? "" : ((DynamicObject)dyData[properties.Name])["Name"].ToString().Trim());//系统值更新时间空值可能是一个空格
                                break;
                            //用户
                            case "E":
                                collog_p.Add(properties.Name,
                                    dyData[properties.Name] == null ? "" : ((DynamicObject)dyData[properties.Name])["Name"].ToString().Trim());//系统值更新时间空值可能是一个空格
                                break;
                            //下拉列表
                            case "F":
                                collog_p.Add(properties.Name, dyData[properties.Name] == null ? "" : dyData[properties.Name].ToString());
                                break;

                        }

                    }
                }
                if (collog_p.Count > 4)
                {
                    valuesfoItem.Add(collog_p);
                }
            }
        }

        /// <summary>
        /// 获取日志记录设置表表体字段(默认有单据编号字段)
        /// </summary>
        /// <param name="billkey">表标识</param>
        /// <param name="colarr">表体字段数组</param>
        /// <returns>表体字段列值</returns>
        private List<string[]> GetColKeyToArray(string billkey, params string[] colarr)
        {
            string sql = @"/*dialect*/ select {0}  bs.F_PAEZ_BILLNOCOL
from PAEZ_t_BillLogColSetup bs
left join PAEZ_BillLogColSetupEntry bse on bse.FID=bs.FID
WHERE BS.F_PAEZ_BILLKEY='{1}'
  AND BS.FDOCUMENTSTATUS='C'";
            string sqlcol = "";
            DataSet tab;
            List<string[]> datacol = new List<string[]>();
            if (colarr.Length > 0)
            {
                for (int i = 0; i < colarr.Length; i++)
                {
                    sqlcol += "bse." + colarr[i] + ",";
                    //if (i != colarr.Length - 1)
                    //    sqlcol += ",";
                }
            }
            tab = DBServiceHelper.ExecuteDataSet(Context, string.Format(sql, sqlcol, billkey));
            if(colarr.Length > 0)
                for (int i = 0; i < colarr.Length; i++)
                    datacol.Add(tab.Tables[0].AsEnumerable().Select(d => d.Field<string>(colarr[i])).ToArray());

            datacol.Add(tab.Tables[0].AsEnumerable().Select(d => d.Field<string>("F_PAEZ_BILLNOCOL")).ToArray());
            return datacol;
        }

        

        



        
        




    }
}
