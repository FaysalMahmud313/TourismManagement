using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TourismManagement
{
    public class PaymentHandler
    {
        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        public PaymentHandler()
        {

        }

        public void ProcessSuccessfulPayment(string paymentID, string bookingID, string travelerID)
        {
            try
            {
                con.Open();
                using (SqlTransaction tran = con.BeginTransaction())
                {
                    try
                    {
                        // 1️⃣ Handle booking type
                        if (bookingID.StartsWith("BKF"))
                        {
                            ProcessFlightBooking(bookingID, travelerID, tran);
                        }
                        else if (bookingID.StartsWith("BKH"))
                        {
                            ProcessHotelBooking(bookingID, tran);
                        }
                        else
                        {
                            throw new Exception("Invalid BookingID format.");
                        }

                        // 2️⃣ Vendor + company share
                        AddVendorPayment(bookingID, tran);

                        // ✅ Commit transaction (DB only)
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        MessageBox.Show("Error updating booking after payment: " + ex.Message,
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return; // exit early
                    }
                }

                // 3️⃣ Send Email AFTER commit
                try
                {
                    if (bookingID.StartsWith("BKF"))
                    {
                        SendPaymentReceiptEmail(travelerID, bookingID, paymentID, con, null);
                    }
                    else if (bookingID.StartsWith("BKH"))
                    {
                        SendHotelPaymentReceiptEmail(travelerID, bookingID, paymentID, con, null);
                    }

                    MessageBox.Show("Payment processed and booking updated successfully!",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Booking saved, but failed to send email: " + ex.Message,
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database connection error: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
            }
        }

        // ✅ Process Flight Bookings
        private void ProcessFlightBooking(string bookingID, string travelerID, SqlTransaction tran)
        {
            string flightID = "", seatNumbers = "";

            string getFlightQuery = @"SELECT ServiceID, SeatNumber FROM Booking_Info WHERE BookingID = @BookingID";
            using (SqlCommand cmd = new SqlCommand(getFlightQuery, con, tran))
            {
                cmd.Parameters.AddWithValue("@BookingID", bookingID);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        flightID = reader["ServiceID"].ToString();
                        seatNumbers = reader["SeatNumber"].ToString();
                    }
                    else
                    {
                        throw new Exception("Flight booking not found.");
                    }
                }
            }

            // Update seats
            if (!string.IsNullOrEmpty(flightID) && !string.IsNullOrEmpty(seatNumbers))
            {
                foreach (string seat in seatNumbers.Split(','))
                {
                    string updateSeatQuery = @"
                UPDATE Seat_Info
                SET IsBooked = 1, BookedBy = @BookedBy, BookingDate = @BookingDate
                WHERE FlightID = @FlightID AND SeatNumber = @SeatNumber";

                    using (SqlCommand cmd = new SqlCommand(updateSeatQuery, con, tran))
                    {
                        cmd.Parameters.AddWithValue("@FlightID", flightID);
                        cmd.Parameters.AddWithValue("@SeatNumber", seat.Trim());
                        cmd.Parameters.AddWithValue("@BookedBy", travelerID);
                        cmd.Parameters.AddWithValue("@BookingDate", DateTime.Now);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            // Update booking status
            string updateBooking = "UPDATE Booking_Info SET BookingStatus = 'Booked' WHERE BookingID = @BookingID";
            using (SqlCommand cmd = new SqlCommand(updateBooking, con, tran))
            {
                cmd.Parameters.AddWithValue("@BookingID", bookingID);
                cmd.ExecuteNonQuery();
            }
        }

        // ✅ Process Hotel Bookings
        private void ProcessHotelBooking(string bookingID, SqlTransaction tran)
        {
            string hotelID = "", roomNumbers = "";

            string getHotelQuery = "SELECT HotelID, RoomNumbers FROM HotelBooking_Info WHERE BookingID = @BookingID";
            using (SqlCommand cmd = new SqlCommand(getHotelQuery, con, tran))
            {
                cmd.Parameters.AddWithValue("@BookingID", bookingID);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        hotelID = reader["HotelID"].ToString();
                        roomNumbers = reader["RoomNumbers"].ToString();
                    }
                    else
                    {
                        throw new Exception("Hotel booking not found.");
                    }
                }
            }

            // Update booking status
            string updateHotel = "UPDATE HotelBooking_Info SET BookingStatus = 'Booked' WHERE BookingID = @BookingID";
            using (SqlCommand cmd = new SqlCommand(updateHotel, con, tran))
            {
                cmd.Parameters.AddWithValue("@BookingID", bookingID);
                cmd.ExecuteNonQuery();
            }

            // Update rooms
            string updateRooms = @"
        UPDATE Room_Info
        SET RoomStatus = 'Booked'
        WHERE HotelID = @HotelID
          AND RoomNumber IN (SELECT value FROM STRING_SPLIT(@RoomNumbers, ','))";

            using (SqlCommand cmd = new SqlCommand(updateRooms, con, tran))
            {
                cmd.Parameters.AddWithValue("@HotelID", hotelID);
                cmd.Parameters.AddWithValue("@RoomNumbers", roomNumbers);
                cmd.ExecuteNonQuery();
            }
        }

        // ✅ Handle Vendor & Company Payment
        private void AddVendorPayment(string bookingId, SqlTransaction tran)
        {
            decimal totalBill = 0, vendorAmount = 0, totalBalance = 0;
            string vendorId = "", walletId = "", paymentId = GeneratePaymentID(con,tran);

            // 1️⃣ Get Total Bill
            string billQuery = "SELECT TotalBill FROM Payment_Info WHERE BookingID=@BookingID";
            using (SqlCommand cmd = new SqlCommand(billQuery, con, tran))
            {
                cmd.Parameters.AddWithValue("@BookingID", bookingId);
                object result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                    throw new Exception("Payment_Info not found for BookingID " + bookingId);
                totalBill = Convert.ToDecimal(result);
            }

            // 2️⃣ Get Vendor ID
            if (bookingId.StartsWith("BKF"))
            {
                string flightId = null;
                using (SqlCommand cmd = new SqlCommand("SELECT ServiceID FROM Booking_Info WHERE BookingID=@BookingID", con, tran))
                {
                    cmd.Parameters.AddWithValue("@BookingID", bookingId);
                    flightId = cmd.ExecuteScalar()?.ToString();
                }

                if (string.IsNullOrEmpty(flightId))
                    throw new Exception("ServiceID not found for booking " + bookingId);

                using (SqlCommand cmd = new SqlCommand("SELECT VendorID FROM Flight_Info WHERE FlightID=@FlightID", con, tran))
                {
                    cmd.Parameters.AddWithValue("@FlightID", flightId);
                    vendorId = cmd.ExecuteScalar()?.ToString();
                }
            }
            else if (bookingId.StartsWith("BKH"))
            {
                string hotelId = null;
                using (SqlCommand cmd = new SqlCommand("SELECT HotelID FROM HotelBooking_Info WHERE BookingID=@BookingID", con, tran))
                {
                    cmd.Parameters.AddWithValue("@BookingID", bookingId);
                    hotelId = cmd.ExecuteScalar()?.ToString();
                }

                if (string.IsNullOrEmpty(hotelId))
                    throw new Exception("HotelID not found for booking " + bookingId);

                using (SqlCommand cmd = new SqlCommand("SELECT VendorID FROM Hotel_Info WHERE HotelID=@HotelID", con, tran))
                {
                    cmd.Parameters.AddWithValue("@HotelID", hotelId);
                    vendorId = cmd.ExecuteScalar()?.ToString();
                }
            }

            if (string.IsNullOrEmpty(vendorId))
                throw new Exception("Vendor not found for booking " + bookingId);

            // 3️⃣ Calculate shares
            vendorAmount = totalBill * 0.85M;
            decimal companyShare = totalBill * 0.15M;

            // 4️⃣ Get or create wallet
            string walletQuery = "SELECT WalletID, TotalEarning FROM Vendor_Wallet WHERE VendorID=@VendorID";
            using (SqlCommand cmd = new SqlCommand(walletQuery, con, tran))
            {
                cmd.Parameters.AddWithValue("@VendorID", vendorId);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        walletId = reader["WalletID"].ToString();
                        totalBalance = Convert.ToDecimal(reader["TotalEarning"]) + vendorAmount;
                    }
                }
            }

            if (!string.IsNullOrEmpty(walletId))
            {
                string updateWallet = "UPDATE Vendor_Wallet SET TotalEarning=@TotalEarning WHERE WalletID=@WalletID";
                using (SqlCommand cmd = new SqlCommand(updateWallet, con, tran))
                {
                    cmd.Parameters.AddWithValue("@TotalEarning", totalBalance);
                    cmd.Parameters.AddWithValue("@WalletID", walletId);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                string insertWallet = @"INSERT INTO Vendor_Wallet (VendorID, TotalEarning) 
                                OUTPUT INSERTED.WalletID 
                                VALUES (@VendorID, @TotalEarning)";
                using (SqlCommand cmd = new SqlCommand(insertWallet, con, tran))
                {
                    cmd.Parameters.AddWithValue("@VendorID", vendorId);
                    cmd.Parameters.AddWithValue("@TotalEarning", vendorAmount);
                    object walletResult = cmd.ExecuteScalar();
                    if (walletResult == null || walletResult == DBNull.Value)
                        throw new Exception("Failed to create Vendor_Wallet for VendorID " + vendorId);
                    walletId = walletResult.ToString();
                    totalBalance = vendorAmount;
                }
            }

            // 5️⃣ Insert vendor payment
            string insertPayment = @"
        INSERT INTO Vendor_Payment (PaymentID, WalletID, BookingID, TotalBill, VendorAmount, TotalBalance, PaymentDate)
        VALUES (@PaymentID, @WalletID, @BookingID, @TotalBill, @VendorAmount, @TotalBalance, @PaymentDate)";
            using (SqlCommand cmd = new SqlCommand(insertPayment, con, tran))
            {
                cmd.Parameters.AddWithValue("@PaymentID", paymentId);
                cmd.Parameters.AddWithValue("@WalletID", walletId);
                cmd.Parameters.AddWithValue("@BookingID", bookingId);
                cmd.Parameters.AddWithValue("@TotalBill", totalBill);
                cmd.Parameters.AddWithValue("@VendorAmount", vendorAmount);
                cmd.Parameters.AddWithValue("@TotalBalance", totalBalance);
                cmd.Parameters.AddWithValue("@PaymentDate", DateTime.Now);
                cmd.ExecuteNonQuery();
            }

            // 6️⃣ Insert company share
            decimal lastCompanyBalance = 0;
            string getCompanyBalance = "SELECT TOP 1 TotalBalance FROM Company_Wallet ORDER BY TransactionID DESC";
            using (SqlCommand cmd = new SqlCommand(getCompanyBalance, con, tran))
            {
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    lastCompanyBalance = Convert.ToDecimal(result);
            }

            decimal newCompanyBalance = lastCompanyBalance + companyShare;
            string insertCompanyWallet = @"INSERT INTO Company_Wallet (ReceivedAmount, TotalBalance, TransactionDate)
                                   VALUES (@ReceivedAmount, @TotalBalance, @TransactionDate)";
            using (SqlCommand cmd = new SqlCommand(insertCompanyWallet, con, tran))
            {
                cmd.Parameters.AddWithValue("@ReceivedAmount", companyShare);
                cmd.Parameters.AddWithValue("@TotalBalance", newCompanyBalance);
                cmd.Parameters.AddWithValue("@TransactionDate", DateTime.Now);
                cmd.ExecuteNonQuery();
            }
        }

        // ✅ Send Email
        private void SendPaymentReceiptEmail(string travelerId, string bookingId, string paymentId, SqlConnection con, SqlTransaction tran)
        {
            try
            {
                string travelerName = "", email = "", flightId = "", fromCity = "", toCity = "",
                       flightTime = "", reportingTime = "", seats = "", bankName = "";
                DateTime flightDate = DateTime.MinValue;
                decimal totalBill = 0;

                // Booking info
                string bookingQuery = @"SELECT ServiceID, SeatNumber FROM Booking_Info WHERE BookingID=@BookingID";
                using (SqlCommand cmd = new SqlCommand(bookingQuery, con, tran))
                {
                    cmd.Parameters.AddWithValue("@BookingID", bookingId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) throw new Exception("Booking not found.");
                        flightId = reader["ServiceID"].ToString();
                        seats = reader["SeatNumber"].ToString();
                    }
                }

                // Flight info
                string flightQuery = @"SELECT FlightDate, FromCity, ToCity, Time, ReportingTime FROM Flight_Info WHERE FlightID=@FlightID";
                using (SqlCommand cmd = new SqlCommand(flightQuery, con, tran))
                {
                    cmd.Parameters.AddWithValue("@FlightID", flightId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) throw new Exception("Flight not found.");
                        flightDate = Convert.ToDateTime(reader["FlightDate"]);
                        fromCity = reader["FromCity"].ToString();
                        toCity = reader["ToCity"].ToString();
                        flightTime = reader["Time"].ToString();
                        reportingTime = reader["ReportingTime"].ToString();
                    }
                }

                // Traveler info
                string travelerQuery = "SELECT Name, Email FROM Traveler_Info WHERE TravelerID=@TravelerID";
                using (SqlCommand cmd = new SqlCommand(travelerQuery, con, tran))
                {
                    cmd.Parameters.AddWithValue("@TravelerID", travelerId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) throw new Exception("Traveler not found.");
                        travelerName = reader["Name"].ToString();
                        email = reader["Email"].ToString();
                    }
                }

                // Payment info
                string paymentQuery = "SELECT TotalBill, PaymentMethod FROM Payment_Info WHERE PaymentID=@PaymentID";
                using (SqlCommand cmd = new SqlCommand(paymentQuery, con, tran))
                {
                    cmd.Parameters.AddWithValue("@PaymentID", paymentId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) throw new Exception("Payment not found.");
                        totalBill = Convert.ToDecimal(reader["TotalBill"]);
                        bankName = reader["PaymentMethod"].ToString();
                    }
                }

                
                string body = $@"
Hello {travelerName},

Your payment has been successfully processed. Here are your trip details:

----------------------------------
Payment Invoice : {paymentId}
Booking ID      : {bookingId}
Flight ID       : {flightId}
Flight Date     : {flightDate:dd-MMM-yyyy}
From            : {fromCity}
To              : {toCity}
Flight Time     : {flightTime}
Reporting Time  : {reportingTime}
Seats           : {seats}
Total Bill      : {totalBill:F2} BDT
Payment Method  : {bankName}
----------------------------------

We wish you a pleasant and safe journey! ✈️ 

Thank you for choosing our service!

Warm regards,
Team Brainstormers
";

                // Send Email
                if (string.IsNullOrWhiteSpace(email)) throw new Exception("Traveler email not found.");

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("teambrainstomers05@gmail.com");
                    mail.To.Add(email);
                    mail.Subject = "Payment Invoice - Brainstormers";
                    mail.Body = body;
                    mail.IsBodyHtml = false;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("teambrainstomers05@gmail.com", "bgeo elqz itqw wyvs"); // ⚠️ Move to config
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }

                MessageBox.Show("Payment invoice sent successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to send email: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private string GeneratePaymentID(SqlConnection con, SqlTransaction tran)
        {
            string paymentID = "";
            bool isUnique = false;
            Random rnd = new Random();

            while (!isUnique)
            {
                
                int number = rnd.Next(1000, 9999);
                paymentID = "VP" + number;

                
                string query = "SELECT COUNT(*) FROM Vendor_Payment WHERE PaymentID=@PaymentID";
                using (SqlCommand cmd = new SqlCommand(query, con, tran))
                {
                    cmd.Parameters.AddWithValue("@PaymentID", paymentID);
                    int count = (int)cmd.ExecuteScalar();
                    if (count == 0)
                        isUnique = true; 
                }
            }

            return paymentID;
        }

        public void SendHotelPaymentReceiptEmail(string travelerId, string bookingId, string paymentId, SqlConnection con, SqlTransaction tran)
        {
            string travelerName = "", email = "";
            string hotelName = "", roomType = "", rooms = "";
            DateTime checkInDate = DateTime.MinValue, checkOutDate = DateTime.MinValue;
            decimal totalBill = 0;
            string bankName = "";

            try
            {
               
                string bookingQuery = @"SELECT HotelID, RoomNumbers, CheckInDate, CheckOutDate 
                                FROM HotelBooking_Info 
                                WHERE BookingID = @BookingID";
                using (SqlCommand cmd = new SqlCommand(bookingQuery, con, tran))
                {
                    cmd.Parameters.AddWithValue("@BookingID", bookingId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            throw new Exception("Hotel booking not found.");

                        hotelName = reader["HotelID"].ToString(); 
                        //roomType = reader["RoomType"].ToString();
                        rooms = reader["RoomNumbers"].ToString();
                        checkInDate = Convert.ToDateTime(reader["CheckInDate"]);
                        checkOutDate = Convert.ToDateTime(reader["CheckOutDate"]);
                    }
                }
                string[] roomList = rooms.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                HashSet<string> roomTypes = new HashSet<string>();

                foreach (var room in roomList)
                {
                    if (room.StartsWith("RS", StringComparison.OrdinalIgnoreCase))
                        roomTypes.Add("Single");
                    else if (room.StartsWith("RD", StringComparison.OrdinalIgnoreCase))
                        roomTypes.Add("Double");
                    else if (room.StartsWith("RT", StringComparison.OrdinalIgnoreCase))
                        roomTypes.Add("Triple");
                    else
                        roomTypes.Add("Unknown");
                }
                roomType = string.Join(", ", roomTypes);

                string hotelQuery = @"SELECT HotelName FROM Hotel_Info WHERE HotelID = @HotelID";
                using (SqlCommand cmd = new SqlCommand(hotelQuery, con, tran))
                {
                    cmd.Parameters.AddWithValue("@HotelID", hotelName);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            throw new Exception("Hotel not found.");
                        hotelName = reader["HotelName"].ToString();
                    }
                }

               
                string travelerQuery = @"SELECT Name, Email FROM Traveler_Info WHERE TravelerID = @TravelerID";
                using (SqlCommand cmd = new SqlCommand(travelerQuery, con, tran))
                {
                    cmd.Parameters.AddWithValue("@TravelerID", travelerId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            throw new Exception("Traveler not found.");
                        travelerName = reader["Name"].ToString();
                        email = reader["Email"].ToString();
                    }
                }

                // 4️⃣ Payment Info
                string paymentQuery = @"SELECT TotalBill, PaymentMethod 
                                FROM Payment_Info 
                                WHERE PaymentID = @PaymentID";
                using (SqlCommand cmd = new SqlCommand(paymentQuery, con, tran))
                {
                    cmd.Parameters.AddWithValue("@PaymentID", paymentId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            throw new Exception("Payment record not found.");
                        totalBill = Convert.ToDecimal(reader["TotalBill"]);
                        bankName = reader["PaymentMethod"].ToString();
                    }
                }

                // 5️⃣ Email body
                string body = $@"
Hello {travelerName},

Your hotel booking payment has been successfully processed. Here are your booking details:

----------------------------------
Payment Invoice : {paymentId}
Booking ID      : {bookingId}
Hotel Name      : {hotelName}
Check-in Date   : {checkInDate:dd-MMM-yyyy}
Check-out Date  : {checkOutDate:dd-MMM-yyyy}
Room Type       : {roomType}
Number of Rooms : {rooms}
Total Bill      : {totalBill:F2} BDT
Payment Method  : {bankName}
----------------------------------

We wish you a pleasant stay! 🏨 

Thank you for choosing our service!

Warm regards,  
Team Brainstormers
";

                // 6️⃣ Send Email
                if (string.IsNullOrWhiteSpace(email))
                    throw new Exception("Traveler email not found.");

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("teambrainstomers05@gmail.com");
                    mail.To.Add(email);
                    mail.Subject = "Hotel Payment Invoice - Brainstormers";
                    mail.Body = body;
                    mail.IsBodyHtml = false;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential(
                            "teambrainstomers05@gmail.com",
                            "bgeo elqz itqw wyvs" // ⚠️ Use secure config, not hardcoded
                        );
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }

                MessageBox.Show("Hotel payment invoice sent successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to send hotel payment receipt: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
