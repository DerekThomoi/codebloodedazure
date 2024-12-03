﻿using AttendanceDatabase.Data;
using AttendanceDatabase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AttendanceDatabase.Controllers
{
    public class DataEntryController : Controller
    {
        private readonly AttendanceDbContext _context;

        public DataEntryController(AttendanceDbContext context)
        {
            _context = context;
        }

        // GET: DailyEntry
        public async Task<IActionResult> DailyEntry()
        {
            // Get today's date
            var today = DateTime.Today;

            // Fetch all event attendance records for today
            var events = await _context.EventAttendanceRecords
                .Where(e => e.Date == today)
                .ToListAsync();

            // Pass today's date and events to the view
            ViewBag.Date = today;
            ViewBag.Events = events;

            return View();
        }

        // GET: EditData
        public async Task<IActionResult> EditData(DateTime entryDate)
        {
            // Retrieve all events for the selected date
            var events = await _context.EventAttendanceRecords
                .Where(e => e.Date == entryDate)
                .ToListAsync();

            // Pass data to the view
            ViewBag.EntryDate = entryDate;
            ViewBag.Events = events;

            return View();
        }

        // POST: UpdateAttendance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAttendance(
            int[] attendanceCounts,
            int[] eventIds,
            DateTime entryDate,
            int[] flaggedEvents)
        {
            // Validate input arrays
            if (attendanceCounts == null || eventIds == null || attendanceCounts.Length != eventIds.Length)
            {
                TempData["ErrorMessage"] = "Input data mismatch.";
                return RedirectToAction("MainMenu", "Menus");
            }

            // Iterate over the attendance records to update them
            for (int i = 0; i < attendanceCounts.Length; i++)
            {
                var count = attendanceCounts[i];
                var eventId = eventIds[i];

                // Find the event attendance record for the selected date
                var eventRecord = await _context.EventAttendanceRecords
                    .FirstOrDefaultAsync(e => e.Id == eventId && e.Date == entryDate);

                if (eventRecord != null)
                {
                    // Update the existing record
                    eventRecord.AttendanceCount = count;

                    // Update the flagged status
                    eventRecord.IsFlagged = flaggedEvents.Contains(eventId);
                }
                else
                {
                    // If the event record isn't found for the given date
                    TempData["ErrorMessage"] = $"Event with ID {eventId} not found for the selected date.";
                    return RedirectToAction("MainMenu", "Menus");
                }
            }

            try
            {
                // Save changes to the database
                await _context.SaveChangesAsync();

                // Use TempData to pass success message
                TempData["SuccessMessage"] = "Attendance updated successfully!";
                return RedirectToAction("MainMenu", "Menus");
            }
            catch (Exception ex)
            {
                // Handle save errors
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("MainMenu", "Menus");
            }
        }


    }
}

