using System;
using System.Collections.Generic;
using System.Text;

namespace BtcIO
{
    struct req
    {
        public string address;
        public int total_received;
        public int total_sent;
        public int balance;
        public int unconfirmed_balance;
        public decimal final_balance;
        public int n_tx;
        public int unconfirmed_n_tx;
        public int final_n_tx;
        public List<txx> txrefs;
        public string tx_url;
    }

    public struct req2
    {
        public string address { get; set; }
        public long total_received { get; set; }
        public long total_sent { get; set; }
        public long balance { get; set; }
        public int unconfirmed_balance { get; set; }
        public decimal final_balance { get; set; }
        public int n_tx { get; set; }
        public int unconfirmed_n_tx { get; set; }
        public int final_n_tx { get; set; }
    }

    struct txx
    {
        public string tx_hash;
        public int block_height;
        public int tx_input_n;
        public int tx_output_n;
        public int value;
        public int ref_balance;
        public bool spent;
        public int irmations;
        public string confirmed;
        public bool double_spend;
    }

    //public struct txx_uncormf
    //{
    //    public string address { get; set; }
    //    public string tx_hash { get; set; }
    //    public int tx_input_n { get; set; }
    //    public int tx_output_n { get; set; }
    //    public int value { get; set; }
    //    public bool spent { get; set; }
    //    public DateTime received { get; set; }
    //    public int confirmations { get; set; }
    //    public bool double_spend { get; set; }
    //    public string preference { get; set; }
    //}

    //public struct req_uncormf
    //{
    //    public string address { get; set; }
    //    public int total_received { get; set; }
    //    public int total_sent { get; set; }
    //    public int balance { get; set; }
    //    public int unconfirmed_balance { get; set; }
    //    public int final_balance { get; set; }
    //    public int n_tx { get; set; }
    //    public int unconfirmed_n_tx { get; set; }
    //    public int final_n_tx { get; set; }
    //    public List<txx_uncormf> unconfirmed_txrefs { get; set; }
    //    public string tx_url { get; set; }
    //}

    public class Input
    {
        public string prev_hash { get; set; }
        public int output_index { get; set; }
        public string script { get; set; }
        public int output_value { get; set; }
        public long sequence { get; set; }
        public List<string> addresses { get; set; }
        public string script_type { get; set; }
        public int age { get; set; }
    }

    public class Output
    {
        public int value { get; set; }
        public string script { get; set; }
        public List<string> addresses { get; set; }
        public string script_type { get; set; }
    }

    public class Tx
    {
        public int block_height { get; set; }
        public int block_index { get; set; }
        public string hash { get; set; }
        public List<string> addresses { get; set; }
        public int total { get; set; }
        public int fees { get; set; }
        public int size { get; set; }
        public string preference { get; set; }
        public string relayed_by { get; set; }
        public string received { get; set; }
        public int ver { get; set; }
        public bool double_spend { get; set; }
        public int vin_sz { get; set; }
        public int vout_sz { get; set; }
        public int confirmations { get; set; }
        public List<Input> inputs { get; set; }
        public List<Output> outputs { get; set; }
    }

    public class BCtcPushResult
    {
        public Tx tx { get; set; }
    }
}
