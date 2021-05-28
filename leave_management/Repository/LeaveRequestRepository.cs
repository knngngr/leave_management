using leave_management.Contracts;
using leave_management.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace leave_management.Repository
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {

        private readonly ApplicationDbContext _db;

        public LeaveRequestRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> Create(LeaveRequest entity)
        {

           await _db.LeaveRequests.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Delete(LeaveRequest entity)
        {

             _db.LeaveRequests.Remove(entity);
            return await Save();
        }

        public async Task<ICollection<LeaveRequest>> FindAll()
        {
            return await _db.LeaveRequests.Include(k=>k.RequestingEmployee).Include(k=>k.ApprovedBy).Include(k=>k.LeaveType).ToListAsync();
        }

        public async Task<LeaveRequest> FindById(int id)
        {
            return await _db.LeaveRequests.Include(k => k.RequestingEmployee).Include(k => k.ApprovedBy).Include(k => k.LeaveType).FirstOrDefaultAsync(k=>k.Id==id);
        }

        public async Task<ICollection<LeaveRequest>> GetLeaveRequestsByEmployee(string id)
        {
            var leaveRequest = await this.FindAll();
            return leaveRequest.Where(k => k.RequestingEmployeeId == id).ToList();
        }

        public async Task<bool> IsExists(int id)
        {
            var exists = await _db.LeaveRequests.AnyAsync(k => k.Id == id);
            return exists;
        }

        public async Task<bool> Save()
        {
            var change =  await _db.SaveChangesAsync();
            return change > 0; 
        }

        public async Task<bool> Update(LeaveRequest entity)
        {
            _db.LeaveRequests.Update(entity);
            return await Save();
        }
    }
}
