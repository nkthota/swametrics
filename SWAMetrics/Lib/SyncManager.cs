using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Npgsql;

namespace SWAMetrics.Lib
{
    public class SyncManager
    {
        public string UpdateRecord(string linkid, string ep1id, string ep2id, string type)
        {
            try
            {
                var connString = "Host=wpsync03;Username=sa;Password=gossippw;Database=gossip_db";
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand($"update identity_map_{linkid} set ignore_update = '{type}' where ep1_entity_id='{ep1id}' and ep2_entity_id='{ep2id}'", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                return "record updated";
            }
            catch(Exception exp)
            {   
                return exp.Message;
            }

        }

        public string IgnoreRecord(string linkid, string ep1id, string ep2id, string type)
        {
            try
            {
                var connString = "Host=wpsync03;Username=sa;Password=gossippw;Database=gossip_db";
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand($"update identity_map_{linkid} set ignore_update = '{type}' where ep1_entity_id='{ep1id}' and ep2_entity_id='{ep2id}'", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                return "record updated";
            }
            catch (Exception exp)
            {
                return exp.Message;
            }

        }

        public string EnableRecord(string linkid, string ep1id, string ep2id, string type)
        {
            try
            {
                var connString = "Host=wpsync03;Username=sa;Password=gossippw;Database=gossip_db";
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand($"update identity_map_{linkid} set ignore_update = '{type}' where ep1_entity_id='{ep1id}' and ep2_entity_id='{ep2id}'", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                return "record updated";
            }
            catch (Exception exp)
            {
                return exp.Message;
            }

        }

        public string UpdateMappingRecord(string link, string mappingid, string entityid, string type)    
        {       
            try
            {
                var connString = "Host=wpsync03;Username=sa;Password=gossippw;Database=gossip_db";
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    if(type =="ep1")
                    {
                        using (var cmd = new NpgsqlCommand($"update identity_map_{link} set ep1_entity_id = '{entityid}' where mapping_id = '{mappingid}'", conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (var cmd = new NpgsqlCommand($"update identity_map_{link} set ep2_entity_id = '{entityid}' where mapping_id = '{mappingid}'", conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }

                }
                return "record updated";
            }
            catch (Exception exp)
            {
                return exp.Message;
            }

        }
    }
}