
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TH;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Config
{
    public interface IConfigTable
    {
        void ProcessCSV(string contents);
    }

    public abstract class ConfigManagerBase<DataType, DerType> : IConfigTable
        where DerType : class
        where DataType : class, new()
    {
        static public DerType instance { private set; get; }

        public Dictionary<int, DataType> dataIdMap
        {
            private set;
            get;
        }

        public ConfigManagerBase()
        {
            dataIdMap = new Dictionary<int, DataType>();
            instance = this as DerType;
        }

        public unsafe virtual void ProcessCSV(string contents)
        {
            dataIdMap.Clear();

            var fieldNameCache = new Dictionary<int, string>();
            var typeInfo = typeof(DataType);
            var idField = typeInfo.GetField("id", BindingFlags.Instance | BindingFlags.Public);

            //第一行保留字段
            //第二行是策划定义的注释
            //第三行正文
            string item, fieldName;
            var lines = contents.Split('\n');
            int j, itemsLength;
            for (int i = 0, length = lines.Length; i < length; ++i)
            {
                //跳过第2,3行
                if (i == 1 || i == 2)
                    continue;

                var items = lines[i].TrimEnd('\r').Split('\t');
                itemsLength = items.Length;
                if (i == 0)
                {
                    for (j = 0; j < itemsLength; ++j)
                    {
                        item = items[j];
                        if (!string.IsNullOrEmpty(item))
                            fieldNameCache.Add(j, item);
                    }
                }
                else
                {
                    //排除空数据
                    if (itemsLength == 0 || string.IsNullOrEmpty(lines[i]))
                        continue;
                    DataType data = new DataType();
                    bool valid = false;
                    int key = 0;
                    for (j = 0; j < itemsLength; ++j)
                    {
                        item = items[j];
                        if (!string.IsNullOrEmpty(item) && fieldNameCache.ContainsKey(j))
                        {
                            fieldName = fieldNameCache[j];

                            var field = typeInfo.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
                            if (field == null)
                            {
                                Debug.LogWarning("Config DataDefine Have No Field :" + fieldName + "/" + typeInfo.Name);
                                continue;
                            }
                            if (field.FieldType == typeof(int))
                            {
                                var dotIndex = item.IndexOf('.');
                                //因为有的表格转过来 可能是123.000 这种 就需要去掉后面.000
                                if (dotIndex != -1)
                                {
                                    //利用正则替换末尾为0或者.
                                    item = Regex.Replace(item, @"[.0]*$", "");
                                }
                                var value = int.Parse(item);
                                field.SetValue(data, value);
                                if (fieldName == "id")
                                {
                                    key = value;
                                }
                            }
                            else if (field.FieldType == typeof(float))
                                field.SetValue(data, float.Parse(item));
                            else if (field.FieldType == typeof(bool))
                                field.SetValue(data, int.Parse(item) == 1);
                            else if (field.FieldType.IsEnum)
                                field.SetValue(data, int.Parse(item));
                            else
                                field.SetValue(data, item);
                            //只要有任意值 则表示 有效
                            valid = true;
                        }
                    }

                    //方便把其他字段转为唯一id,默认情况 就是id
                    if (valid)
                    {
                        if (key == 0)
                        {
                            GLog.LogException(string.Format("配置表{0} 第{1}行没有对id赋值", typeInfo.Name, (i + 1)));
                        }
                        var newKey = GetDataTypeKey(ref data, key);
                        dataIdMap.Add(newKey, data);
                    }
                }
            }
        }

        protected virtual int GetDataTypeKey(ref DataType data, int oldKey)
        {
            return oldKey;
        }
    }
}