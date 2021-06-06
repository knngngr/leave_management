using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using leave_management.Contracts;
using leave_management.Data;
using leave_management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace leave_management.Controllers
{

    [Authorize(Roles = "Administrator")]
    public class LeaveAllocationController : Controller
    {
        // GET: LeaveAllocationController

        private readonly ILeaveTypeRepository _leaverepo;
        private readonly ILeaveAllocationRepository _leaveallocationrepo;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<Employee> _userManager;

        public LeaveAllocationController(ILeaveTypeRepository leaverepo, ILeaveAllocationRepository leaveallocationrepo,UserManager<Employee> userManager ,IMapper mapper, IUnitOfWork unitOfWork )
        {
            _leaveallocationrepo = leaveallocationrepo;
            _leaverepo = leaverepo;
            _userManager = userManager;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ActionResult> Index()
        {
    
            var leaveTypes = await _unitOfWork.LeaveTypes.FindAll();
            var mappedLeaveTypes = _mapper.Map<List<LeaveType>, List<LeaveTypeVM>>(leaveTypes.ToList());

            var model = new CreateLeaveAllocationVM
            {

                LeaveTypes = mappedLeaveTypes,
                NumberUpdated = 0
            };

            return View(model);



        }

        public async Task<ActionResult> SetLeave(int id )
        {
            var leavetypes =  await _unitOfWork.LeaveTypes.Find(k=>k.Id==id);
            var Employees = await _userManager.GetUsersInRoleAsync("Employee");
            var period = DateTime.Now.Year;
            foreach(var emp  in  Employees)
            {
                //var isAllocated = await _leaveallocationrepo.CheckAllocation(id, emp.Id);
                if (await _unitOfWork.LeaveAllocations.IsExists(k=>k.EmployeeId==emp.Id && k.LeaveTypeId == leavetypes.Id && k.Period == period))
                    continue;

                var allocation = new LeaveAllocationVM
                {
                    DateCreated = DateTime.Now,
                    EmployeeId = emp.Id,
                    LeaveTypeId = id,
                    NumberOfDays = leavetypes.DefaultDays,
                    Period = DateTime.Now.Year

                };
                var leaveallocation = _mapper.Map<LeaveAllocation>(allocation);
               await _unitOfWork.LeaveAllocations.Create(leaveallocation);
                await _unitOfWork.Save();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<ActionResult> ListEmployees()
        {
            var Employees = await _userManager.GetUsersInRoleAsync("Employee");
            var model = _mapper.Map<List<EmployeeVM>>(Employees);
            return View(model);
        }


        // GET: LeaveAllocationController/Details/5
        public async Task<ActionResult> Details(string  id)
        {
            var period = DateTime.Now.Year;
            var employee =  _mapper.Map<EmployeeVM>( await _userManager.FindByIdAsync(id));
         
            var allocations = _mapper.Map<List<LeaveAllocationVM>>( await _unitOfWork.LeaveAllocations.FindAll(k=>k.EmployeeId==id && k.Period==period,includes:new List<string> {"LeaveType" }));

            var model = new ViewAllocationVM { Employee = employee, LeaveAllocations = allocations };

            return View(model);
        }

        // GET: LeaveAllocationController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LeaveAllocationController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveAllocationController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {

            var leaveallocation = await _unitOfWork.LeaveAllocations.Find(k=>k.Id==id);
            var model = _mapper.Map<EditLeaveAllocationVM>(leaveallocation);


            return View(model);
        }

        // POST: LeaveAllocationController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditLeaveAllocationVM model)
        {
            try
            {

                if(!ModelState.IsValid)
                {
                    return View(model);
                }

                var record = await _unitOfWork.LeaveAllocations.Find(k=>k.Id==model.Id);
                record.NumberOfDays = model.NumberOfDays;
               
                _unitOfWork.LeaveAllocations.Update(record);
                await _unitOfWork.Save();
                
                return RedirectToAction(nameof(Details),new {id=model.EmployeeId });
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveAllocationController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveAllocationController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        protected override void Dispose(bool disposing)
        {
            _unitOfWork.Dispose();
            base.Dispose(disposing);
        }
    }
}
