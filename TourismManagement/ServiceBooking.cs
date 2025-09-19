using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TourismManagement
{
    public class ServiceBooking
    {

        SqlConnection con = new SqlConnection("Data Source=faysal\\sqlexpress01;Initial Catalog=TravelManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");

        
        public void BookingFlight(string flightID, int numberOfSeat, string travelerId, DateTime date)
        {
            string travelerName = "";
            string bookingID = GenerateFlightBookingID(); // master booking ID
            decimal farePerSeat = 0, totalBill = 0, vat = 0, discount = 0, netPayable = 0, totalvat = 0;
            List<string> bookedSeats = new List<string>();

            try
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();

                try
                {
                    // Traveler name
                    SqlCommand namecmd = new SqlCommand(
                        "SELECT Name FROM Traveler_Info WHERE TravelerID = @TravelerID", con, tran);
                    namecmd.Parameters.AddWithValue("@TravelerID", travelerId);
                    object result = namecmd.ExecuteScalar();
                    travelerName = result?.ToString() ?? "Unknown";

                    // Fare per seat
                    SqlCommand fareCmd = new SqlCommand(
                        "SELECT Fare FROM Flight_Info WHERE FlightID = @FlightID", con, tran);
                    fareCmd.Parameters.AddWithValue("@FlightID", flightID);
                    farePerSeat = Convert.ToDecimal(fareCmd.ExecuteScalar());

                    // Total bill
                    totalBill = farePerSeat * numberOfSeat;
                    vat = totalBill * 0.015m;
                    totalvat = totalBill + vat;  // 1.5% VAT
                    discount = totalvat >= 50000 ? totalBill * 0.05m : 0;
                    netPayable = totalBill + vat - discount;

                  
                    string seatQuery = @"SELECT TOP (@SeatsToBook) SeatNumber 
                                 FROM Seat_Info 
                                 WHERE FlightID = @FlightID AND IsBooked = 0 
                                 ORDER BY CAST(SUBSTRING(SeatNumber, 2, LEN(SeatNumber)) AS INT) ASC";

                    SqlCommand seatCmd = new SqlCommand(seatQuery, con, tran);
                    seatCmd.Parameters.AddWithValue("@SeatsToBook", numberOfSeat);
                    seatCmd.Parameters.AddWithValue("@FlightID", flightID);

                    using (SqlDataReader reader = seatCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bookedSeats.Add(reader["SeatNumber"].ToString());
                        }
                    }

                    if (bookedSeats.Count < numberOfSeat)
                    {
                        MessageBox.Show("Not enough seats available for this flight.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        tran.Rollback();
                        return;
                    }
                    string seatList = string.Join(",", bookedSeats);
                    SqlCommand masterCmd = new SqlCommand(@"
    INSERT INTO Booking_Info 
        (BookingID, ServiceID,TravelerID, BookingDate, TotalBill, BookingStatus,SeatNumber)
    VALUES 
        (@BookingID, @ServiceID, @TravelerID, @BookingDate, @TotalBill, @BookingStatus,@SeatNumber)", con, tran);

                    masterCmd.Parameters.AddWithValue("@BookingID", bookingID);
                    masterCmd.Parameters.AddWithValue("@ServiceID", flightID);
                    masterCmd.Parameters.AddWithValue("@TravelerID", travelerId);
                    masterCmd.Parameters.AddWithValue("@BookingDate", DateTime.Now);
                    masterCmd.Parameters.AddWithValue("@TotalBill", netPayable);
                    masterCmd.Parameters.AddWithValue("@SeatNumber", seatList);
                    masterCmd.Parameters.AddWithValue("@BookingStatus", "Pending"); 

                    masterCmd.ExecuteNonQuery();
                    
                    
                   /* SqlCommand detailCmd = new SqlCommand(@"
                INSERT INTO Booking_Detail (BookingID, FlightID, SeatNumber)
                VALUES (@BookingID, @FlightID, @SeatNumbers)", con, tran);

                    detailCmd.Parameters.AddWithValue("@BookingID", bookingID);
                    detailCmd.Parameters.AddWithValue("@FlightID", flightID);
                    detailCmd.Parameters.AddWithValue("@SeatNumbers", seatList);

                    detailCmd.ExecuteNonQuery();*/

                    tran.Commit();
                    gifMassageBox.Show("Booked! Complete Payment to Confirm!");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    MessageBox.Show("Error booking: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message);
            }
            finally
            {
                con.Close();
            }


            PrepareBill pb = new PrepareBill();
            pb.SetBillDetails(travelerName, flightID, bookingID, bookedSeats, totalBill, totalvat, discount, netPayable,date,travelerId);
            pb.Show();
            
        }

       

        public void BookingHotel(string hotelID, string travelerId, DateTime checkIn, DateTime checkOut, int numberOfRooms,string RoomType)
        {

            string travelerName = "";
            string bookingID = GenerateHotelBookingID();
            decimal pricePerNight = 0, totalBill = 0, vat = 0, discount = 0, netPayable = 0, totalvat = 0;
            int days = 0;
            
            string roomType= RoomType;
            

            try
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();

                try
                {
                    // Traveler name
                    SqlCommand nameCmd = new SqlCommand(
                        "SELECT Name FROM Traveler_Info WHERE TravelerID = @TravelerID", con, tran);
                    nameCmd.Parameters.AddWithValue("@TravelerID", travelerId);
                    object result = nameCmd.ExecuteScalar();
                    travelerName = result?.ToString() ?? "Unknown";

                    

                    string availableRoomsQuery = @"
                SELECT TOP(@RequiredRooms) RoomID, RoomNumber, PricePerNight
                FROM Room_Info
                WHERE HotelID = @HotelID
                  AND RoomType = @RoomType
                  AND RoomStatus = 'Available'";

                    SqlCommand availCmd = new SqlCommand(availableRoomsQuery, con, tran);
                    availCmd.Parameters.AddWithValue("@HotelID", hotelID);
                    availCmd.Parameters.AddWithValue("@RoomType", roomType);
                    availCmd.Parameters.AddWithValue("@RequiredRooms", numberOfRooms);

                    List<string> roomIDs = new List<string>();
                    List<string> roomNumbers = new List<string>();
                    List<decimal> roomPrices = new List<decimal>();

                    using (SqlDataReader reader = availCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            roomIDs.Add(reader["RoomID"].ToString());
                            roomNumbers.Add(reader["RoomNumber"].ToString());
                            roomPrices.Add(Convert.ToDecimal(reader["PricePerNight"]));
                        }
                    }

                   
                    if (roomIDs.Count < numberOfRooms)
                    {
                        MessageBox.Show("Not enough rooms available for the selected type.", "Insufficient Rooms", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        tran.Rollback();
                        return;
                    }

                   
                    days = (checkOut - checkIn).Days;
                    if (days <= 0) days = 1;

                    
                    totalBill = 0;
                    for (int i = 0; i < roomPrices.Count; i++)
                    {
                        totalBill += roomPrices[i] * days;
                    }

                    vat = totalBill * 0.015m;//1.5%
                    totalvat = vat+totalBill;
                    discount = totalvat >= 50000 ? totalvat * 0.1m : 0; 
                    netPayable = totalvat - discount;

                    
                    string insertBookingQuery = @"
                INSERT INTO HotelBooking_Info 
                (BookingID, HotelID, TravelerID, CheckInDate, CheckOutDate, RoomsBooked, TotalBill, VAT, Discount, NetPayable, RoomNumbers) 
                VALUES (@BookingID, @HotelID, @TravelerID, @CheckInDate, @CheckOutDate, @RoomsBooked, @TotalBill, @VAT, @Discount, @NetPayable, @RoomNumbers)";

                    SqlCommand insertCmd = new SqlCommand(insertBookingQuery, con, tran);
                    insertCmd.Parameters.AddWithValue("@BookingID", bookingID);
                    insertCmd.Parameters.AddWithValue("@HotelID", hotelID);
                    insertCmd.Parameters.AddWithValue("@TravelerID", travelerId);
                    insertCmd.Parameters.AddWithValue("@CheckInDate", checkIn);
                    insertCmd.Parameters.AddWithValue("@CheckOutDate", checkOut);
                    insertCmd.Parameters.AddWithValue("@RoomsBooked", numberOfRooms);
                    insertCmd.Parameters.AddWithValue("@TotalBill", totalBill);
                    insertCmd.Parameters.AddWithValue("@VAT", totalvat);
                    insertCmd.Parameters.AddWithValue("@Discount", discount);
                    insertCmd.Parameters.AddWithValue("@NetPayable", netPayable);
                    insertCmd.Parameters.AddWithValue("@RoomNumbers", string.Join(",", roomNumbers));

                    insertCmd.ExecuteNonQuery();

                  

                    tran.Commit();
                    gifMassageBox.Show("Booked! Complete Payment to Confirm!");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    MessageBox.Show("Error booking hotel: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message);
            }
            finally
            {
                con.Close();
            }

            
            PrepareBill pb = new PrepareBill();
            pb.SetHotelBillDetails(travelerName, hotelID, bookingID, checkIn, checkOut,
                           numberOfRooms, days, pricePerNight, roomType, totalBill, totalvat, discount, netPayable,travelerId);
            pb.Show();
        }


        private string GenerateFlightBookingID()
        {
            string bookingID = "";
            bool isUnique = false;
            Random rnd = new Random();

            try
            {
                con.Open();

                while (!isUnique)
                {
                   
                    
                    int number = rnd.Next(10000, 99999);
                    bookingID = "BKF" + number;

                 
                    string checkQuery = "SELECT COUNT(*) FROM Booking_Info WHERE BookingID = @BookingID";
                    using (SqlCommand cmd = new SqlCommand(checkQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@BookingID", bookingID);

                        object result = cmd.ExecuteScalar();
                        int count = 0;

                        if (result != null && result != DBNull.Value)
                        {
                            count = Convert.ToInt32(result);
                        }

                        if (count == 0)
                        {
                           
                            isUnique = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating Booking ID: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (con.State == System.Data.ConnectionState.Open)
                    con.Close();
            }

            return bookingID;
        }

       
        private string GenerateHotelBookingID()
        {
            string bookingId = "";
            bool isUnique = false;
            Random rnd = new Random();

            try
            {
                con.Open();

                while (!isUnique)
                {
                 
                    
                    int number = rnd.Next(10000, 99999);
                    bookingId = "BKH" + number;

                    string checkQuery = "SELECT COUNT(*) FROM HotelBooking_Info WHERE BookingID = @BookingID";
                    using (SqlCommand cmd = new SqlCommand(checkQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@BookingID", bookingId);

                        object result = cmd.ExecuteScalar();
                        int count = 0;

                        if (result != null && result != DBNull.Value)
                        {
                            count = Convert.ToInt32(result);
                        }

                        if (count == 0)
                        {
                           
                            isUnique = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating Hotel Booking ID: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (con.State == System.Data.ConnectionState.Open)
                    con.Close();
            }

            return bookingId;
        }
    }
}
