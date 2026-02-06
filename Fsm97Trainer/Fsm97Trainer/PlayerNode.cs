using FSM97Lib;
using System;
using System.Diagnostics;
using System.Text;
using System.Web;

namespace Fsm97Trainer
{
    public class PlayerNode : IObjectWithPersonName
    {
        public int NodeAddress { get; set; }
        public int DataAddress { get; set; }
        public int NextNode { get; set; }
        public int PreviousNode { get; set; }
        public Player Data { get; set; }
        public string LastName { get => ((IObjectWithPersonName)Data).LastName; set => ((IObjectWithPersonName)Data).LastName = value; }
        public string FirstName { get => ((IObjectWithPersonName)Data).FirstName; set => ((IObjectWithPersonName)Data).FirstName = value; }
        public override string ToString()
        {
            return string.Format("{0:x}->{1:x}: {2},{3}", NodeAddress, DataAddress, LastName, FirstName);
        }
        public void WritePlayer(Process process,Encoding encoding)
        {
            int playerDataAddress = DataAddress;
            Player player = Data;
            byte[] playerDataBuffer = NativeMethods.ReadBytes(process, playerDataAddress, 0x76);
            byte[] lastNameBuffer = new byte[0x13];
            byte[] firstNameBuffer = new byte[0x18];

            var lastNameBytes = encoding.GetBytes(Data.LastName);
            var firstNameBytes = encoding.GetBytes(Data.FirstName);

            Buffer.BlockCopy(lastNameBytes, 0, lastNameBuffer, 0, lastNameBytes.Length> lastNameBuffer.Length? lastNameBuffer.Length: lastNameBytes.Length);
            Buffer.BlockCopy(firstNameBytes, 0, firstNameBuffer, 0, firstNameBytes.Length > firstNameBuffer.Length ? firstNameBuffer.Length : firstNameBytes.Length);
            Buffer.BlockCopy(lastNameBuffer, 0, playerDataBuffer, 0x1c, lastNameBuffer.Length);
            Buffer.BlockCopy(firstNameBuffer, 0, playerDataBuffer,0x4, firstNameBuffer.Length);

            playerDataBuffer[0x2f] = (byte)player.Nationality;
            playerDataBuffer[0x30] = (byte)player.Position;
            playerDataBuffer[0x31] = (byte)player.Status;
            playerDataBuffer[0x32] = (byte)player.Number;
            Buffer.BlockCopy(player.Attributes, 0, playerDataBuffer, 0x33, (int)PlayerAttribute.Count);
            playerDataBuffer[0x4b] = (byte)player.Form;
            playerDataBuffer[0x4c] = (byte)player.Moral;
            playerDataBuffer[0x4d] = (byte)player.Energy;

            var dayValue = (ushort) (Data.BirthDay - new DateTime(1899, 12, 30)).Days;
            var dayValueBytes = BitConverter.GetBytes(dayValue);
            Buffer.BlockCopy(dayValueBytes, 0, playerDataBuffer, 0x52,2);

            var salaryBytes = BitConverter.GetBytes(player.Salary);
            salaryBytes.CopyTo(playerDataBuffer, 0x60);

            playerDataBuffer[0x6e] = (byte)player.GamesThisSeason;

            playerDataBuffer[0x73] = (byte)player.Goals;
            playerDataBuffer[0x74] = (byte)player.MVP;
            playerDataBuffer[0x75] = (byte)player.ContractWeeks;
            NativeMethods.WriteBytes(process, playerDataAddress, playerDataBuffer, 0,(uint)playerDataBuffer.Length);

        }
        public void WritePlayerStatus(Process process)
        {
            NativeMethods.WriteByte(process, DataAddress + 0x31, (byte)Data.Status);
        }
        public void WritePlayerBirthDate(Process process)
        {
            NativeMethods.WriteUShort(process, DataAddress + 0x52, Data.BirthDateOffset);
        }

        public void WritePlayerNames(Process process, Encoding encoding)
        {
            NativeMethods.WriteString(process, Data.LastName, DataAddress + 0x1c, encoding, 0x13);
            NativeMethods.WriteString(process, Data.FirstName, DataAddress + 0x4, encoding, 0x18);
        }
        public void ReadPlayer(Process process,int playerDataAddress, Team team, Encoding encoding, int currentDate)
        {
            this.DataAddress = playerDataAddress;
            Player player = new Player();

            player.FirstName = NativeMethods.ReadString
                        (process, playerDataAddress + 4, encoding, 0x18);
            player.LastName = NativeMethods.ReadString(process, playerDataAddress + 0x1c, encoding, 0x13);
            byte[] bytes = NativeMethods.ReadBytes(process, playerDataAddress, 0x76);
            player.Nationality = bytes[0x2f];
            player.Position = bytes[0x30];
            player.Status = bytes[0x31];
            player.Number = bytes[0x32];

            Buffer.BlockCopy(bytes, 0x33, player.Attributes, 0, (int)PlayerAttribute.Count);
            player.Form = bytes[0x4b];
            player.Moral = bytes[0x4c];
            player.Energy = bytes[0x4d];
            //salary
            player.Salary = BitConverter.ToDouble(bytes, 0x60);
            player.GamesThisSeason = bytes[0x6e];
            player.Goals = bytes[0x73];
            player.MVP = bytes[0x74];
            player.ContractWeeks = bytes[0x75];
            player.Team = team;
            player.PositionRating = (int)PositionRatings.GetPositionRatingDouble(player.Position, player.Attributes);
            player.UpdateBestPosition();

            player.BirthDateOffset = NativeMethods.ReadUShort(process, playerDataAddress + 0x52);
            DateTime currentDateTime = Player.dateOffsetBase.AddDays(currentDate);
            player.UpdateAge(currentDateTime);
            Data = player;
        }
        byte[] formMoralEnergyBuffer = new byte[3];
        public void WritePlayerFormMoralEnergy(Process process)
        {
            formMoralEnergyBuffer[0] = (byte)Data.Form;
            formMoralEnergyBuffer[1] = (byte)Data.Moral;
            formMoralEnergyBuffer[2] = (byte)Data.Energy;
            NativeMethods.WriteBytes(process, DataAddress + 0x4b, formMoralEnergyBuffer, 0, 3);
        }
        public void WritePlayerStrengths(Process process)
        {
            formMoralEnergyBuffer[0] = (byte)Data.Stamina;
            formMoralEnergyBuffer[1] = (byte)Data.Strength;
            formMoralEnergyBuffer[2] = (byte)Data.Fitness;
            NativeMethods.WriteBytes(process, DataAddress + 0x36, formMoralEnergyBuffer, 0, 3);
        }
        public void WritePlayerPosition(Process process)
        {

            NativeMethods.WriteByte(process, DataAddress + 0x30, (byte)Data.Position);
        }
        public void WritePlayerContractWeeks(Process process)
        {

            NativeMethods.WriteByte(process, DataAddress + 0x75, (byte)Data.ContractWeeks);
        }

    }
}
