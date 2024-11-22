﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using WolfDen.Domain.Entity;
using WolfDen.Domain.Enums;
using WolfDen.Infrastructure.Data;


namespace WolfDen.Application.Requests.Commands.Attendence.CloseAttendance
{
    public class CloseAttendanceCommandHandler : IRequestHandler<CloseAttendanceCommand, int>
    {
        private readonly WolfDenContext _context;
        public CloseAttendanceCommandHandler(WolfDenContext context)
        {
            _context = context;
        }
        public async Task<int> Handle(CloseAttendanceCommand request, CancellationToken cancellationToken)
        {
            int minWorkDuration = 360;
            List<LeaveType> leaveTypes = await _context.LeaveType.ToListAsync(cancellationToken);
            List<Employee> employees = await _context.Employees.ToListAsync(cancellationToken);
            DateOnly monthStart = new DateOnly(request.Year, request.Month, 1);
            DateTime currentDat = DateTime.Now;
            int year = currentDat.Year;
            int month = currentDat.Month;
            int day = currentDat.Day;
            DateOnly attendanceClosingDate = new DateOnly(year, month, day);
            List<DailyAttendence> attendanceRecords = await _context.DailyAttendence
               .Where(x => x.Date >= monthStart && x.Date <= attendanceClosingDate)
               .ToListAsync(cancellationToken);

            List<Holiday> holidays = await _context.Holiday
                   .Where(x => x.Date >= monthStart && x.Date <= attendanceClosingDate)
                   .ToListAsync(cancellationToken);

            List<LeaveRequest> leaveRequests = await _context.LeaveRequests
                  .Where(x => x.FromDate <= attendanceClosingDate && x.ToDate >= monthStart &&
                              x.LeaveRequestStatusId == LeaveRequestStatus.Approved)
                  .ToListAsync(cancellationToken);
            foreach (Employee employee in employees)
            {
                int lopCount = 0;
                int incompleteShiftCount = 0;
                string lopdays = "";
                string incompleteShiftDays = "";
                for (var currentDate = monthStart; currentDate <= attendanceClosingDate; currentDate = currentDate.AddDays(1))
                {
                    DailyAttendence attendanceRecord = attendanceRecords.FirstOrDefault(x => x.EmployeeId == employee.Id && x.Date == currentDate);
                    if (attendanceRecord is not null)
                    {
                        if (attendanceRecord.InsideDuration < minWorkDuration)
                        {
                            incompleteShiftDays += currentDate.ToString("yyyy-MM-dd") + ",";
                            Console.WriteLine(incompleteShiftDays);
                            incompleteShiftCount++;
                        }
                    }
                    else
                    {
                        Holiday holiday = holidays.FirstOrDefault(x => x.Date == currentDate);
                        if (holiday is not null)
                        {
                            if (holiday.Type is not AttendanceStatus.NormalHoliday)
                            {
                                LeaveRequest leaveRequestForHoliday = leaveRequests.FirstOrDefault(x => x.EmployeeId == employee.Id && x.FromDate <= currentDate && x.ToDate >= currentDate);

                                if (leaveRequestForHoliday is null)
                                {
                                    lopdays += currentDate.ToString("yyyy-MM-dd") + ",";
                                    lopCount++;
                                }
                            }
                        }
                        else
                        {
                            LeaveRequest leaveRequest = leaveRequests.FirstOrDefault(x => x.EmployeeId == employee.Id && x.FromDate <= currentDate && x.ToDate >= currentDate);
                            if (leaveRequest is null)
                            {
                                lopdays += currentDate.ToString("yyyy-MM-dd") + ",";
                                lopCount++;
                            }
                        }
                    }

                }
                LOP lop = new LOP(attendanceClosingDate, employee.Id, lopCount, incompleteShiftCount, lopdays, incompleteShiftDays);
                await _context.AddAsync(lop);
                await _context.SaveChangesAsync(cancellationToken);

            }
            DateTime date = new DateTime(request.Year, request.Month, 1);
            AttendenceClose attendenceClose = new AttendenceClose(attendanceClosingDate, true, date.ToString("MMMM"), request.Year);
            await _context.AddAsync(attendenceClose);
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
