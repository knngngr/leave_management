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
using Microsoft.AspNetCore.Mvc;

namespace leave_management.Controllers
{
    [Authorize(Roles ="Administrator")]
    public class LeaveTypesController : Controller
    {
        private readonly ILeaveTypeRepository _repo;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public LeaveTypesController(ILeaveTypeRepository repo, IMapper mapper,IUnitOfWork unitofWork)
        {
            _repo = repo;
            _mapper = mapper;
            _unitOfWork = unitofWork;

        }


        // GET: LeaveTypeController
        public async Task<ActionResult> Index()
        {
            var leaveTypes = await _unitOfWork.LeaveTypes.FindAll();

            var model = _mapper.Map<List<LeaveType>, List<LeaveTypeVM>>(leaveTypes.ToList());

            return View(model);
        }

        // GET: LeaveTypeController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var isExists = await _unitOfWork.LeaveTypes.IsExists(k=>k.Id==id);
            if (!isExists)
                return NotFound();

            var leavType = await _unitOfWork.LeaveTypes.Find(k=>k.Id==id);
            var model = _mapper.Map<LeaveTypeVM>(leavType);


            return View(model);
        }

        // GET: LeaveTypeController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LeaveTypeController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(LeaveTypeVM model)
        {
            try
            {


                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var leaveType = _mapper.Map<LeaveType>(model);
                leaveType.DateCreated = DateTime.Now;
                await _unitOfWork.LeaveTypes.Create(leaveType);
                await _unitOfWork.Save();
               

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Something went wrong...");
                return View();
            }
        }

        // GET: LeaveTypeController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var isExists = await _unitOfWork.LeaveTypes.IsExists(k=>k.Id==id);
            if (!isExists)
            {
                return NotFound();
            }
            var leaveType = await _unitOfWork.LeaveTypes.Find(k=>k.Id==id);
            var model = _mapper.Map<LeaveTypeVM>(leaveType);

            return View(model);
        }

        // POST: LeaveTypeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(LeaveTypeVM model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                var leaveType = _mapper.Map<LeaveType>(model);
                _unitOfWork.LeaveTypes.Update(leaveType);
                await _unitOfWork.Save();
                //if (!isSuccess)
                //{
                //    ModelState.AddModelError("", "Something went wrong...");
                //    return View(model);
                //}


                return RedirectToAction(nameof(Index));
            }
            catch
            {

                ModelState.AddModelError("", "Something went wrong...");
                return View();
            }
        }

        // GET: LeaveTypeController/Delete/5
        //public async Task<ActionResult> Delete(int id)
        //{
        //    var leaveType = await _unitOfWork.LeaveTypes.Find(expression:k=>k.Id==id);
        //    if (leaveType == null)
        //    {
        //        return NotFound();
        //    }
        //    _unitOfWork.LeaveTypes.Delete(leaveType);
        //    await _unitOfWork.Save();

        //    //if (!isSuccess)
        //    //{
        //    //    return BadRequest();
        //    //}
        //    return RedirectToAction(nameof(Index));
        //}

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, LeaveTypeVM model)
        {
            try
            {
                var leaveType = await _unitOfWork.LeaveTypes.Find(expression: k => k.Id == id);
                if (leaveType == null)
                {
                    return NotFound();
                }
                _unitOfWork.LeaveTypes.Delete(leaveType);
                await _unitOfWork.Save();

                //if (!isSuccess)
                //{
                //    return BadRequest();
                //}
                return RedirectToAction(nameof(Index));

            }
            catch
            {
                return View(model);
            }
        }

        protected override void Dispose(bool disposing)
        {
            _unitOfWork.Dispose();
            base.Dispose(disposing);
        }
    }
}
