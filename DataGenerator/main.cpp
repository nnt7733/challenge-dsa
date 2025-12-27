/*
    MINRIDE DATA GENERATOR
    Chuong trinh sinh du lieu mau cho he thong MinRide
    
    Bien dich: g++ -o generator main.cpp -std=c++11
    Chay: ./generator
*/

#include <iostream>
#include <fstream>
#include <vector>
#include <string>
#include <cstdlib>
#include <ctime>
#include <cmath>
#include <iomanip>
#include <sstream>

using namespace std;

// ==================== CAU HINH ====================
const int SO_TAI_XE = 100;
const int SO_KHACH_HANG = 100;
const int SO_CHUYEN_DI = 500;
const string THU_MUC_OUTPUT = "../MinRide/Data";  // Thay doi neu can
// ==================================================

// Mang ten tieng Viet
const string HO[] = {"Nguyen", "Tran", "Le", "Pham", "Hoang", "Vo", "Dang", "Bui", "Ngo", "Truong"};
const string TEN_DEM[] = {"Van", "Thi", "Duc", "Minh", "Quang", "Thanh", "Manh", "Quoc", "Hong", "Tuan"};
const string TEN[] = {"An", "Binh", "Cuong", "Dung", "Em", "Phong", "Giang", "Hai", "Khoa", "Lam",
                      "Hoa", "Lan", "Linh", "Nga", "Huong", "Tam", "Tuan", "Hung", "Duc", "Thao"};
const string QUAN[] = {"Quan 1", "Quan 3", "Quan 5", "Quan 7", "Quan 10",
                       "Quan Binh Thanh", "Quan Go Vap", "Quan Thu Duc", "Quan Phu Nhuan", "Quan Tan Binh"};

// Ham tien ich
int randomInt(int min, int max) { return min + rand() % (max - min + 1); }
double randomDouble(double min, double max) { return min + (double)rand() / RAND_MAX * (max - min); }
double lam_tron(double x) { return round(x * 10) / 10; }

string sinh_ten() {
    return string(HO[randomInt(0, 9)]) + " " + TEN_DEM[randomInt(0, 9)] + " " + TEN[randomInt(0, 19)];
}

string sinh_timestamp(int ngay_truoc) {
    time_t now = time(nullptr);
    now -= ngay_truoc * 86400 + randomInt(0, 23) * 3600;
    tm* t = localtime(&now);
    stringstream ss;
    ss << (1900 + t->tm_year) << "-" << setfill('0') << setw(2) << (1 + t->tm_mon) << "-"
       << setw(2) << t->tm_mday << "T" << setw(2) << t->tm_hour << ":"
       << setw(2) << t->tm_min << ":" << setw(2) << t->tm_sec;
    return ss.str();
}

int main() {
    srand(time(nullptr));
    
    cout << "\n";
    cout << "========================================\n";
    cout << "   MINRIDE DATA GENERATOR\n";
    cout << "========================================\n\n";
    
    // ========== SINH TAI XE ==========
    string file_driver = THU_MUC_OUTPUT + "/drivers.csv";
    ofstream f1(file_driver);
    f1 << "ID,Name,Rating,X,Y,TotalRides\n";
    
    cout << "Dang sinh " << SO_TAI_XE << " tai xe...\n";
    for (int i = 1; i <= SO_TAI_XE; i++) {
        f1 << i << "," << sinh_ten() << "," 
           << fixed << setprecision(1) << lam_tron(randomDouble(3.5, 5.0)) << ","
           << lam_tron(randomDouble(0, 10)) << "," << lam_tron(randomDouble(0, 10)) << ","
           << randomInt(10, 80) << "\n";
    }
    f1.close();
    cout << "  -> " << file_driver << "\n";
    
    // ========== SINH KHACH HANG ==========
    string file_customer = THU_MUC_OUTPUT + "/customers.csv";
    ofstream f2(file_customer);
    f2 << "ID,Name,District,X,Y\n";
    
    cout << "Dang sinh " << SO_KHACH_HANG << " khach hang...\n";
    for (int i = 1; i <= SO_KHACH_HANG; i++) {
        f2 << i << "," << sinh_ten() << "," << QUAN[randomInt(0, 9)] << ","
           << fixed << setprecision(1) << lam_tron(randomDouble(0, 10)) << "," 
           << lam_tron(randomDouble(0, 10)) << "\n";
    }
    f2.close();
    cout << "  -> " << file_customer << "\n";
    
    // ========== SINH CHUYEN DI ==========
    string file_ride = THU_MUC_OUTPUT + "/rides.csv";
    ofstream f3(file_ride);
    f3 << "RideId,CustomerId,DriverId,Distance,Fare,Timestamp,Status\n";
    
    cout << "Dang sinh " << SO_CHUYEN_DI << " chuyen di...\n";
    for (int i = 1; i <= SO_CHUYEN_DI; i++) {
        double dist = lam_tron(randomDouble(2, 12));
        f3 << i << "," << randomInt(1, SO_KHACH_HANG) << "," << randomInt(1, SO_TAI_XE) << ","
           << fixed << setprecision(1) << dist << "," << setprecision(0) << (dist * 12000) << ","
           << sinh_timestamp(randomInt(1, 30)) << "," << (randomInt(1, 10) <= 8 ? "CONFIRMED" : "CANCELLED") << "\n";
    }
    f3.close();
    cout << "  -> " << file_ride << "\n";
    
    cout << "\n========================================\n";
    cout << "   HOAN THANH!\n";
    cout << "========================================\n\n";
    
    return 0;
}
