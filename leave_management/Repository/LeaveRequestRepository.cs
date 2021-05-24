using leave_management.Contracts;
using leave_management.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Repository
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {

        private readonly ApplicationDbContext _db;

        public LeaveRequestRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool Create(LeaveRequest entity)
        {

            _db.LeaveRequests.Add(entity);
            return Save();
        }

        public bool Delete(LeaveRequest entity)
        {

            _db.LeaveRequests.Remove(entity);
            return Save();
        }

        public ICollection<LeaveRequest> FindAll()
        {
            return _db.LeaveRequests.Include(k=>k.RequestingEmployee).Include(k=>k.ApprovedBy).Include(k=>k.LeaveType).ToList();
        }

        public LeaveRequest FindById(int id)
        {
            return _db.LeaveRequests.Include(k => k.RequestingEmployee).Include(k => k.ApprovedBy).Include(k => k.LeaveType).FirstOrDefault(k=>k.Id==id);
        }

        public ICollection<LeaveRequest> GetLeaveRequestsByEmployee(string id)
        {
            return this.FindAll().Where(k => k.RequestingEmployeeId == id).ToList();
        }

        public bool IsExists(int id)
        {
            var exists = _db.LeaveRequests.Any(k => k.Id == id);
            return exists;
        }

        public bool Save()
        {
            var change = _db.SaveChanges();
            return change > 0; 
        }

        public bool Update(LeaveRequest entity)
        {
            _db.LeaveRequests.Update(entity);
            return Save();
        }
    }
}
