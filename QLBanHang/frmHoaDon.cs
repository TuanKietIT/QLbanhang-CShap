using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using QLBanHang.Class;
using COMExcel = Microsoft.Office.Interop.Excel;
namespace QLBanHang
{
    public partial class frmHoaDon : Form
    {
        DataTable tblCTHDB; //Bảng chi tiết hoá đơn bán
        public frmHoaDon()
        {
            InitializeComponent();
        }

        private void frmHoaDon_Load(object sender, EventArgs e)
        {
            btnThem.Enabled = true;
            btnLuu.Enabled = false;
            btnXoa.Enabled = false;
            btnInHoaDon.Enabled = false;    
            txtMaHoaDon.ReadOnly = true;
            txtTenNhanVien.ReadOnly = true;
            txtTenKhach.ReadOnly = true;
            txtDiaChi.ReadOnly = true;
            txtDienThoai.ReadOnly = true;
            txtTenSanPham.ReadOnly = true;
            txtDonGia.ReadOnly = true;
            txtThanhTien.ReadOnly = true;
            txtTongTien.ReadOnly = true;
            txtGiamGia.Text = "0";
            txtTongTien.Text = "0";
            functions.FillCombo("SELECT MaKhach, TenKhach FROM tblKhach", cboMaKhach, "MaKhach", "MaKhach");
            cboMaKhach.SelectedIndex = -1;
            functions.FillCombo("SELECT MaNhanVien, TenNhanVien FROM tblNhanVien", cboMaNhanVien, "MaNhanVien", "TenKhach");
            cboMaNhanVien.SelectedIndex = -1;
            functions.FillCombo("SELECT MaSanPham, TenSanPham FROM tblSanPham", cboMaSanPham, "MaSanPham", "MaSanPham");
            cboMaSanPham.SelectedIndex = -1;
            //Hiển thị thông tin của một hóa đơn được gọi từ form tìm kiếm
            if (txtMaHoaDon.Text != "")
            {
                LoadInfoHoaDon();
                btnXoa.Enabled = true;
                btnInHoaDon.Enabled = true;
            }
            LoadDataGridView();
        }
        private void LoadDataGridView()
        {
            string sql;
            sql = "SELECT a.MaHang, b.TenSanPham, a.SoLuong, b.DonGiaBan, a.GiamGia,a.ThanhTien FROM tblCTHoaDon AS a, tblSanPham AS b WHERE a.MaHoaDon = N'" + txtMaHoaDon.Text + "' AND a.MaHang=b.MaSanPham";
            tblCTHDB = functions.GetDataToTable(sql);
            dgvHoaDon.DataSource = tblCTHDB;
            dgvHoaDon.Columns[0].HeaderText = "Mã hàng";
            dgvHoaDon.Columns[1].HeaderText = "Tên hàng";
            dgvHoaDon.Columns[2].HeaderText = "Số lượng";
            dgvHoaDon.Columns[3].HeaderText = "Đơn giá";
            dgvHoaDon.Columns[4].HeaderText = "Giảm giá %";
            dgvHoaDon.Columns[5].HeaderText = "Thành tiền";
            dgvHoaDon.Columns[0].Width = 80;
            dgvHoaDon.Columns[1].Width = 130;
            dgvHoaDon.Columns[2].Width = 80;
            dgvHoaDon.Columns[3].Width = 90;
            dgvHoaDon.Columns[4].Width = 90;
            dgvHoaDon.Columns[5].Width = 90;
            dgvHoaDon.AllowUserToAddRows = false;
            dgvHoaDon.EditMode = DataGridViewEditMode.EditProgrammatically;
        }
        private void LoadInfoHoaDon()
        {
            string str;
            str = "SELECT NgayBan FROM tblHoaDon WHERE MaHoaDon = N'" + txtMaHoaDon.Text + "'";
            dtpNgayBan.Value =DateTime.Parse(functions.GetFieldValues(str));
            str = "SELECT MaNhanVien FROM tblHoaDon WHERE MaHoaDon = N'" + txtMaHoaDon.Text + "'";
            cboMaNhanVien.Text = functions.GetFieldValues(str);
            str = "SELECT MaKhach FROM tblHoaDon WHERE MaHoaDon = N'" + txtMaHoaDon.Text + "'";
            cboMaKhach.Text = functions.GetFieldValues(str);
            str = "SELECT TongTien FROM tblHoaDon WHERE MaHoaDon = N'" + txtMaHoaDon.Text + "'";
            txtTongTien.Text = functions.GetFieldValues(str);
            lblBangChu.Text = "Bằng chữ: " + functions.ChuyenSoSangChuoi(Double.Parse(txtTongTien.Text));
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            btnXoa.Enabled = false;
            btnLuu.Enabled = true;
            btnInHoaDon.Enabled = false;
            btnThem.Enabled = false;
            ResetValues();
            txtMaHoaDon.Text = functions.CreateKey("HDB");
            LoadDataGridView();
        }
        private void ResetValues()
        {
            txtMaHoaDon.Text = "";
            dtpNgayBan.Value = DateTime.Now;
            cboMaNhanVien.Text = "";
            cboMaKhach.Text = "";
            txtTongTien.Text = "0";
            lblBangChu.Text = "Bằng chữ: ";
            cboMaSanPham.Text = "";
            txtSoLuong.Text = "";
            txtGiamGia.Text = "0";  
            txtThanhTien.Text = "0";
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            string sql;
            double sl, SLcon, tong, Tongmoi;
            sql = "SELECT MaHoaDon FROM tblHoaDon WHERE MaHoaDon=N'" + txtMaHoaDon.Text + "'";
            if (!functions.CheckKey(sql))
            {
                // Mã hóa đơn chưa có, tiến hành lưu các thông tin chung
                // Mã HDBan được sinh tự động do đó không có trường hợp trùng khóa
                if (cboMaNhanVien.Text.Length == 0)
                {
                    MessageBox.Show("Bạn phải nhập nhân viên", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    cboMaNhanVien.Focus();
                    return;
                }
                if (cboMaKhach.Text.Length == 0)
                {
                    MessageBox.Show("Bạn phải nhập khách hàng", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    cboMaKhach.Focus();
                    return;
                }
                sql = "INSERT INTO tblHoaDon(MaHoaDon, NgayBan, MaNhanVien, MaKhach, TongTien) VALUES (N'" + txtMaHoaDon.Text.Trim() + "','" +
                        dtpNgayBan.Value + "',N'" + cboMaNhanVien.SelectedValue + "',N'" +
                        cboMaKhach.SelectedValue + "'," + txtTongTien.Text + ")";
                functions.RunSQL(sql);
            }
            // Lưu thông tin của các mặt hàng
            if (cboMaSanPham.Text.Trim().Length == 0)
            {
                MessageBox.Show("Bạn phải nhập mã hàng", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cboMaSanPham.Focus();
                return;
            }
            if ((txtSoLuong.Text.Trim().Length == 0) || (txtSoLuong.Text == "0"))
            {
                MessageBox.Show("Bạn phải nhập số lượng", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtSoLuong.Text = "";
                txtSoLuong.Focus();
                return;
            }
            if (txtGiamGia.Text.Trim().Length == 0)
            {
                MessageBox.Show("Bạn phải nhập giảm giá", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtGiamGia.Focus();
                return;
            }
            sql = "SELECT MaHang FROM tblCTHoaDon WHERE MaHang=N'" + cboMaSanPham.SelectedValue + "' AND MaHoaDon = N'" + txtMaHoaDon.Text.Trim() + "'";
            if (functions.CheckKey(sql))
            {
                MessageBox.Show("Mã hàng này đã có, bạn phải nhập mã khác", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ResetValuesHang();
                cboMaSanPham.Focus();
                return;
            }
            // Kiểm tra xem số lượng hàng trong kho còn đủ để cung cấp không?
            sl = Convert.ToDouble(functions.GetFieldValues("SELECT SoLuong FROM tblSanPham WHERE MaSanPham = N'" + cboMaSanPham.SelectedValue + "'"));
            if (Convert.ToDouble(txtSoLuong.Text) > sl)
            {
                MessageBox.Show("Số lượng mặt hàng này chỉ còn " + sl, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtSoLuong.Text = "";
                txtSoLuong.Focus();
                return;
            }
            sql = "INSERT INTO tblCTHoaDon(MaHoaDon,MaHang,SoLuong,DonGia, GiamGia,ThanhTien) VALUES(N'" + txtMaHoaDon.Text.Trim() + "',N'" + cboMaSanPham.SelectedValue + "'," + txtSoLuong.Text + "," + txtDonGia.Text + "," + txtGiamGia.Text + "," + txtThanhTien.Text + ")";
            functions.RunSQL(sql);
            LoadDataGridView();
            // Cập nhật lại số lượng của mặt hàng vào bảng tblHang
            SLcon = sl - Convert.ToDouble(txtSoLuong.Text);
            sql = "UPDATE tblSanPham SET SoLuong =" + SLcon + " WHERE MaSanPham= N'" + cboMaSanPham.SelectedValue + "'";
            functions.RunSQL(sql);
            // Cập nhật lại tổng tiền cho hóa đơn bán
            tong = Convert.ToDouble(functions.GetFieldValues("SELECT TongTien FROM tblHoaDon WHERE MaHoaDon = N'" + txtMaHoaDon.Text + "'"));
            Tongmoi = tong + Convert.ToDouble(txtThanhTien.Text);
            sql = "UPDATE tblHoaDon SET TongTien =" + Tongmoi + " WHERE MaHoaDon = N'" + txtMaHoaDon.Text + "'";
            functions.RunSQL(sql);
            txtTongTien.Text = Tongmoi.ToString();
            lblBangChu.Text = "Bằng chữ: " + functions.ChuyenSoSangChuoi(Double.Parse(Tongmoi.ToString()));
            ResetValuesHang();
            btnXoa.Enabled = true;
            btnThem.Enabled = true;
            btnInHoaDon.Enabled = true;
        }
        private void ResetValuesHang()
        {
            cboMaSanPham.Text = "";
            txtSoLuong.Text = "";
            txtGiamGia.Text = "0";
            txtThanhTien.Text = "0";
        }

        private void cboMaSanPham_SelectedIndexChanged(object sender, EventArgs e)
        {
            string str;
            if (cboMaSanPham.Text == "")
            {
                txtTenSanPham.Text = "";
                txtDonGia.Text = "";
            }
            // Khi chọn mã hàng thì các thông tin về hàng hiện ra
            str = "SELECT TenSanPham FROM tblSanPham WHERE MaSanPham =N'" + cboMaSanPham.SelectedValue + "'";
            txtTenSanPham.Text = functions.GetFieldValues(str);
            str = "SELECT DonGiaBan FROM tblSanPham WHERE MaSanPham =N'" + cboMaSanPham.SelectedValue + "'";
            txtDonGia.Text = functions.GetFieldValues(str);
        }

        private void cboMaKhach_SelectedIndexChanged(object sender, EventArgs e)
        {
            string str;
            if (cboMaKhach.Text == "")
            {
                txtTenKhach.Text = "";
                txtDiaChi.Text = "";
                txtDienThoai.Text = "";
            }
            //Khi chọn Mã khách hàng thì các thông tin của khách hàng sẽ hiện ra
            str = "Select TenKhach from tblKhach where MaKhach = N'" + cboMaKhach.SelectedValue + "'";
            txtTenKhach.Text = functions.GetFieldValues(str);
            str = "Select DiaChi from tblKhach where MaKhach = N'" + cboMaKhach.SelectedValue + "'";
            txtDiaChi.Text = functions.GetFieldValues(str);
            str = "Select DienThoai from tblKhach where MaKhach= N'" + cboMaKhach.SelectedValue + "'";
            txtDienThoai.Text = functions.GetFieldValues(str);
        }

        private void txtSoLuong_TextChanged(object sender, EventArgs e)
        {
            //Khi thay đổi số lượng thì thực hiện tính lại thành tiền
            double tt, sl, dg, gg;
            if (txtSoLuong.Text == "")
                sl = 0;
            else
                sl = Convert.ToDouble(txtSoLuong.Text);
            if (txtGiamGia.Text == "")
                gg = 0;
            else
                gg = Convert.ToDouble(txtGiamGia.Text);
            if (txtDonGia.Text == "")
                dg = 0;
            else
                dg = Convert.ToDouble(txtDonGia.Text);
            tt = sl * dg - sl * dg * gg / 100;
            txtThanhTien.Text = tt.ToString();
        }

        private void txtGiamGia_TextChanged(object sender, EventArgs e)
        {
            // Khi thay đổi giảm giá thì tính lại thành tiền
            double tt, sl, dg, gg;
            if (txtSoLuong.Text == "")
                sl = 0;
            else
                sl = Convert.ToDouble(txtSoLuong.Text);
            if (txtGiamGia.Text == "")
                gg = 0;
            else
                gg = Convert.ToDouble(txtGiamGia.Text);
            if (txtDonGia.Text == "")
                dg = 0;
            else
                dg = Convert.ToDouble(txtDonGia.Text);
            tt = sl * dg - sl * dg * gg / 100;
            txtThanhTien.Text = tt.ToString();
        }

        private void cboMaHoaDon_DropDown(object sender, EventArgs e)
        {
            functions.FillCombo("SELECT MaHoaDon FROM tblHoaDon", cboMaHoaDon, "MaHoaDon", "MaHoaDon");
            cboMaHoaDon.SelectedIndex = -1;
        }

       
    }
}
