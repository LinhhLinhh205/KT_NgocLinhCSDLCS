using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
namespace KT_NgocLinh
{
    public partial class Form1 : Form
    {
        string strcon = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=..\..\..\data\QLSV.mdb";
        DataSet ds = new DataSet();
        OleDbDataAdapter adpSinhVien, adpKhoa, adpKetQua, adpMonHoc;
        OleDbCommandBuilder cmdMonHoc;
        BindingSource bs = new BindingSource();
        int stt = 0;
        public Form1()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            bs.CurrentChanged += Bs_CurrentChanged;
        }

        private void Bs_CurrentChanged(object sender, EventArgs e)
        {
            lblStt.Text = bs.Position + 1 + "/" + bs.Count;
            if(bs.Current != null)
            {
                if(bs.Current is DataRowView dataRowView)
                {
                    string maMH = dataRowView["MaMH"].ToString();
                    txtMaMH.Text = maMH;
                    txtTongSV.Text = TongSSV(maMH).ToString();
                    txtDiemMax.Text = TongMax(maMH).ToString();
                }
            }
            
        }

        private Double TongMax(string MMH)
        {
            double kq = 0;
            Object td = ds.Tables["KETQUA"].Compute("max(diem)", "MaMH='" + MMH + "'");
            if (td == DBNull.Value)
                kq = 0;
            else
                kq = Convert.ToDouble(td);
            return kq;
        }

        private Double TongSSV(string MMH)
        {
            double kq = 0;
            Object td = ds.Tables["KETQUA"].Compute("count(MaSV)", "MaMH='" + MMH + "'");
            if (td == DBNull.Value)
                kq = 0;
            else
                kq = Convert.ToDouble(td);
            return kq;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            khoitaodoituong();
            docdulieu();
            khoitaobindingSource();
            mocnoiquanhe();
            lienketdieukhhien();
            bdnMonHoc.BindingSource = bs;
        }

        private void lienketdieukhhien()
        {
            foreach(Control ctl in this.Controls)
            {
                if (ctl is TextBox && ctl.Name != "txtLoaiMH" && ctl.Name!="txtDiemMax" && ctl.Name !="txtTongSV")
                    ctl.DataBindings.Add("text", bs, ctl.Name.Substring(3), true);
                
            }
            Binding bdLoai = new Binding("text", bs, "LoaiMH", true);
            bdLoai.Format += BdLoai_Format;
            bdLoai.Parse += BdLoai_Parse;
            txtLoaiMH.DataBindings.Add(bdLoai);
        }

        private void BdLoai_Parse(object sender, ConvertEventArgs e)
        {
            if (e.Value == null) return;
            e.Value = e.Value.ToString().ToUpper() == "Bắt buộc" ? true : false;
        }

        private void BdLoai_Format(object sender, ConvertEventArgs e)
        {
            if (e.Value == DBNull.Value || e.Value == null) return;
            e.Value =(Boolean) e.Value ? "Bắt buộc" : "Tùy chọn";
        }

        private void mocnoiquanhe()
        {
            ds.Relations.Add("KH_SV", ds.Tables["KHOA"].Columns["MaKH"], ds.Tables["SINHVIEN"].Columns["MaKH"], true);
            ds.Relations.Add("SV_KQ", ds.Tables["SINHVIEN"].Columns["MaSV"], ds.Tables["KETQUA"].Columns["MaSV"], true);
            ds.Relations.Add("KQ_MH", ds.Tables["MONHOC"].Columns["MaMH"], ds.Tables["KETQUA"].Columns["MaMH"], true);
            ds.Relations["KH_SV"].ChildKeyConstraint.DeleteRule = Rule.None;
            ds.Relations["SV_KQ"].ChildKeyConstraint.DeleteRule = Rule.None;
            ds.Relations["KQ_MH"].ChildKeyConstraint.DeleteRule = Rule.None;
        }

        private void btnSau_Click(object sender, EventArgs e)
        {
            bs.MoveNext();
        }

        private void btnDau_Click(object sender, EventArgs e)
        {
            bs.MoveFirst();
        }

        private void btnTruoc_Click(object sender, EventArgs e)
        {
            bs.MovePrevious();
        }

        private void btnCuoi_Click(object sender, EventArgs e)
        {
            bs.MoveLast();
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            txtMaMH.ReadOnly = false;
            bs.AddNew();
            txtMaMH.Focus();
        }

        private void btnGhi_Click(object sender, EventArgs e)
        {
            if (txtMaMH.ReadOnly == false)
            {
                DataRow rmh = ds.Tables["MONHOC"].Rows.Find(txtMaMH.Text);
                if (rmh != null)
                {
                    MessageBox.Show("MaMH bị trùng", "Lỗi",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    txtMaMH.Focus();
                    return;
                }
            }
            bs.EndEdit();
            int n = adpMonHoc.Update(ds, "MONHOC");
            if (n > 0)
            {
                MessageBox.Show("Cập nhật (Thêm/Sửa) thành công!", "Thông báo");
            }
            txtMaMH.ReadOnly = true;
        }

        private void btnKhong_Click(object sender, EventArgs e)
        {
            bs.CancelEdit();
            txtMaMH.ReadOnly = false;
            bs.Position = stt;
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            DataRow rsv = (bs.Current as DataRowView).Row;
            DataRow[] mang_dong = rsv.GetChildRows("KQ_MH");
            if (mang_dong.Length > 0)
                MessageBox.Show("Không xoá MH nay","Lỗi",MessageBoxButtons.OKCancel,MessageBoxIcon.Error);
            else
            {
                DialogResult tl;
                tl = MessageBox.Show("Bạn chắc chắn xoá MH này không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (tl == DialogResult.Yes)
                {
                    bs.RemoveCurrent();
                    int n = adpMonHoc.Update(ds, "MONHOC");
                    if (n > 0)
                        MessageBox.Show("Xoá MH thành công");
                }
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn có muốn thoát không?", "Xác nhận", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void khoitaobindingSource()
        {
            bs.DataSource = ds;
            bs.DataMember = "MONHOC";
        }

        private void docdulieu()
        {
            
            adpSinhVien.FillSchema(ds, SchemaType.Source, "SINHVIEN");
            adpSinhVien.Fill(ds, "SINHVIEN");

            adpKhoa.FillSchema(ds, SchemaType.Source, "KHOA");
            adpKhoa.Fill(ds, "KHOA");

            adpKetQua.FillSchema(ds, SchemaType.Source, "KETQUA");
            adpKetQua.Fill(ds, "KETQUA");

            adpMonHoc.FillSchema(ds, SchemaType.Source, "MONHOC");
            adpMonHoc.Fill(ds, "MONHOC");
        }

        private void khoitaodoituong()
        {

            adpKhoa = new OleDbDataAdapter("select * from khoa", strcon);
            adpKetQua = new OleDbDataAdapter("select * from ketqua", strcon);
            adpMonHoc = new OleDbDataAdapter("select * from monhoc", strcon);
            adpSinhVien = new OleDbDataAdapter("select * from sinhvien", strcon);
            cmdMonHoc = new OleDbCommandBuilder(adpMonHoc);
        }
    }
}
