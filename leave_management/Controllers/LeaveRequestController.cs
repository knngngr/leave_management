using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using leave_management.Contracts;
using leave_management.Data;
using leave_management.Models;
using leave_management.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace leave_management.Controllers
{
    [Authorize]
    public class LeaveRequestController : Controller
    {
        // GET: LeaveRequestController
        //private readonly ILeaveRequestRepository _leaveRequestRepo;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;
        //private readonly ILeaveTypeRepository _leaveTypeRepo;
        //private readonly ILeaveAllocationRepository _leaveAllocationRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        public LeaveRequestController(ILeaveTypeRepository leaveTypeRepo, ILeaveAllocationRepository leaveAllocationRepo, ILeaveRequestRepository leaveRequestRepo, UserManager<Employee> userManager, IMapper mapper,IUnitOfWork unitOfWork,IEmailSender emailSender)
        {
            //_leaveTypeRepo = leaveTypeRepo;
            //_leaveRequestRepo = leaveRequestRepo;
            _userManager = userManager;
            _mapper = mapper;
            //_leaveAllocationRepo = leaveAllocationRepo;
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;

        }
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Index()
        {
            var leaveRequests = await _unitOfWork.LeaveRequests.FindAll();
            var leaveRequestModel = _mapper.Map<List<LeaveRequestVM>>(leaveRequests);

            var model = new AdminLeaveRequestViewVM
            {
                TotalRequests = leaveRequestModel.Count,
                ApprovedRequests = leaveRequestModel.Count(k => k.Approved == true),
                PendingRequests = leaveRequestModel.Count(q => q.Approved == null),
                RejectedRequests = leaveRequestModel.Count(q => q.Approved == false),
                LeaveRequests = leaveRequestModel

            };



            return View(model);
        }

        // GET: LeaveRequestController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            //var leaveRequest = _leaveRequestRepo.FindById(id);
            var leaveRequest = await _unitOfWork.LeaveRequests.Find(k => k.Id == id);
            var model = _mapper.Map<LeaveRequestVM>(leaveRequest);


            return View(model);
        }

        public async Task<ActionResult> ApproveRequest(int id)
        {

            try
            {



                var user = await _userManager.GetUserAsync(User);
                var leaveRequest = await _unitOfWork.LeaveRequests.Find(k => k.Id == id); //await _leaveRequestRepo.FindById(id);

                var employeeid = leaveRequest.RequestingEmployeeId;
                var leaveTypeId = leaveRequest.LeaveTypeId;
              //  var allocation =  await _leaveAllocationRepo.GetLeaveAllocationsByEmployeeAndType(employeeid, leaveTypeId);

                var period = DateTime.Now.Year;
                var allocation = await _unitOfWork.LeaveAllocations.Find(q => q.EmployeeId == employeeid && q.Period == period && q.LeaveTypeId == leaveTypeId);


                int daysRequested = (int)(leaveRequest.EndDate - leaveRequest.StartDate).TotalDays;
                allocation.NumberOfDays = allocation.NumberOfDays - daysRequested;

                leaveRequest.Approved = true;
                leaveRequest.ApprovedById = user.Id;
                leaveRequest.DateActioned = DateTime.Now;

                //await _leaveRequestRepo.Update(leaveRequest);
                //await _leaveAllocationRepo.Update(allocation);


                _unitOfWork.LeaveRequests.Update(leaveRequest);
                _unitOfWork.LeaveAllocations.Update(allocation);
                await _unitOfWork.Save();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {

                return RedirectToAction(nameof(Index));
            }


        }
        public async Task<ActionResult> RejectRequest(int id)
        {
            try
            {



                var user = await _userManager.GetUserAsync(User);
                var leaveRequest = await _unitOfWork.LeaveRequests.Find(k => k.Id == id);  //await _leaveRequestRepo.FindById(id);
                leaveRequest.Approved = false;
                leaveRequest.ApprovedById = user.Id;
                leaveRequest.DateActioned = DateTime.Now;

                //var isSuccess =  await _leaveRequestRepo.Update(leaveRequest);
                //if (!isSuccess)
                //{
                //    return RedirectToAction(nameof(Index));
                //}

                _unitOfWork.LeaveRequests.Update(leaveRequest);
                await _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {

                return RedirectToAction(nameof(Index));
            }
        }
        // GET: LeaveRequestController/Create
        public async Task<ActionResult> Create()
        {
            var leaveTypes = await _unitOfWork.LeaveTypes.FindAll();

            var leaveTypeItems = leaveTypes.Select(q => new SelectListItem
            {

                Text = q.Name,
                Value = q.Id.ToString()


            });

            var model = new CreateLeaveRequestVM
            {

                LeaveTypes = leaveTypeItems
            };


            return View(model);
        }


        public async Task<ActionResult> MyLeave()
        {


            var employee = await _userManager.GetUserAsync(User);
            var employeeid = employee.Id;
            //var employeeAllocations = await _leaveAllocationRepo.GetLeaveAllocationsByEmployee(employeeid);
            var employeeAllocations = await _unitOfWork.LeaveAllocations.FindAll(k => k.EmployeeId == employeeid);

            //var employeeLeaveRequests = await _leaveRequestRepo.GetLeaveRequestsByEmployee(employeeid);
            var employeeLeaveRequests = await _unitOfWork.LeaveRequests.FindAll(k => k.RequestingEmployeeId == employeeid);

            var leaveAllocationList = _mapper.Map<List<LeaveAllocationVM>>(employeeAllocations);
            var leaveRequestList = _mapper.Map<List<LeaveRequestVM>>(employeeLeaveRequests);


            var empObject = new EmployeeLeaveRequestVM
            {
             LeaveAllocations = leaveAllocationList,
             LeaveRequests=leaveRequestList

            };


            return View(empObject);
        }

        public async Task<ActionResult> CancelRequest(int id)
        {

            var request = await _unitOfWork.LeaveRequests.Find(k=>k.Id==id);
            request.Cancelled = true;
            int numberOfDays = (int)(request.EndDate - request.StartDate).TotalDays;

            //var allocation = await _leaveAllocationRepo.GetLeaveAllocationsByEmployeeAndType(request.RequestingEmployeeId, request.LeaveTypeId);
            //allocation.NumberOfDays = allocation.NumberOfDays + numberOfDays;

             _unitOfWork.LeaveRequests.Update(request);
            await _unitOfWork.Save();
            return RedirectToAction("MyLeave");
        }


        // POST: LeaveRequestController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateLeaveRequestVM model)
        {
            try
            {


                var startDate = Convert.ToDateTime(model.StartDate);
                var endDate = Convert.ToDateTime(model.EndDate);
                var period = DateTime.Now.Year;
                //var leaveTypes = await _leaveTypeRepo.FindAll();
                var leaveTypes = await _unitOfWork.LeaveTypes.FindAll();
                var leaveTypeItems = leaveTypes.Select(q => new SelectListItem
                {

                    Text = q.Name,
                    Value = q.Id.ToString()


                });

                model.LeaveTypes = leaveTypeItems;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }


                if (DateTime.Compare(startDate, endDate) > 0)
                {
                    ModelState.AddModelError("", "Start Date cannot be further in the future than the End Date");
                    return View(model);
                }

                var employee = await _userManager.GetUserAsync(User);
                //var allocations = await _leaveAllocationRepo.GetLeaveAllocationsByEmployeeAndType(employee.Id, model.LeaveTypeId);
                var allocations = await _unitOfWork.LeaveAllocations.Find(q => q.EmployeeId == employee.Id && q.Period == period && q.LeaveTypeId == model.LeaveTypeId);
                int daysRequested = (int)(endDate.Date - startDate.Date).TotalDays;

                if (daysRequested > allocations.NumberOfDays)
                {

                    ModelState.AddModelError("", "You dont have sufficient days for this request ");
                    return View(model);
                }


                var leaveRequestModel = new LeaveRequestVM
                {

                    RequestingEmployeeId = employee.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    Approved = null,
                    DateRequested = DateTime.Now,
                    DateActioned = DateTime.Now,
                    LeaveTypeId = model.LeaveTypeId,
                    RequestComments=model.RequestComments

                };


                var leaveRequest = _mapper.Map<LeaveRequest>(leaveRequestModel);
                //var isSuccess = await _leaveRequestRepo.Create(leaveRequest);
                await _unitOfWork.LeaveRequests.Create(leaveRequest);
                await _unitOfWork.Save();
                //if (!isSuccess)
                //{
                //    ModelState.AddModelError("", "Something went wrong with submitting form ");
                //    return View(model);
                //}

               await _emailSender.SendEmailAsync("admin@localhost.com","New Leave Request",$"Please review this leave Request. <a href='UrlOfApp/{leaveRequest.Id}'>Click Here </a>");

                return RedirectToAction("MyLeave");
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", "Something went wrong");

                return View(model);
            }
        }

        // GET: LeaveRequestController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
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

        // GET: LeaveRequestController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Delete/5
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
