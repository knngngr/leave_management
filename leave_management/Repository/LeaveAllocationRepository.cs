using leave_management.Contracts;
using leave_management.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Repository
{
    public class LeaveAllocationRepository : ILeaveAllocationRepository
    {

        private readonly ApplicationDbContext _db;

        public LeaveAllocationRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool CheckAllocation(int leaveTypeId, string EmployeeId)
        {
            return _db.LeaveAllocations.Where(k => k.LeaveTypeId == leaveTypeId && k.EmployeeId==EmployeeId && k.Period == DateTime.Now.Year).Any();
        }

        public bool Create(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Add(entity);
            return Save();
        }

        public bool Delete(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Remove(entity);
            return Save();
        }

        public ICollection<LeaveAllocation> FindAll()
        {
            return _db.LeaveAllocations.Include(k=>k.LeaveType).Include(k=>k.Employee).ToList();
        }

        public LeaveAllocation FindById(int id)
        {
            return _db.LeaveAllocations.Include(k => k.LeaveType).Include(k => k.Employee).FirstOrDefault(k=>k.Id==id);
        }

        public ICollection<LeaveAllocation> GetLeaveAllocationsByEmployee(string id)
        {
            var period = DateTime.Now.Year;

            return FindAll().Where(q => q.EmployeeId == id && q.Period == period).ToList();
        }

        public bool IsExists(int id)
        {
            var exists = _db.LeaveAllocations.Any(k => k.Id == id);
            return exists;
        }

        public bool Save()
        {
            var chg = _db.SaveChanges();
            return chg > 0;
        }

        public bool Update(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Update(entity);
            return Save();
        }
    }
}
