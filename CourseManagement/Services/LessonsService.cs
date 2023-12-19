using System.Drawing.Printing;
using AutoMapper;
using CourseManagement.Data;
using CourseManagement.Data.Entities;
using CourseManagement.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CourseManagement.Services
{
    public class LessonsService : ILessonsService
    {
        private readonly CourseDbContext _context;
        private readonly IMapper _mapper;

        public LessonsService(CourseDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<int> Create(LessonRequest request)
        {
            var lesson = _mapper.Map<Lesson>(request);
            _context.Add(lesson);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson != null)
            {
                _context.Lessons.Remove(lesson);
            }
            return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<LessonViewModel>> GetAllFilter(string sortOrder, string currentFilter, string searchString, int? courseId, int? pageNumber, int pageSize)
        {
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            var lessons = from m in _context.Lessons select m;

            if (courseId != null)
            {
                lessons = lessons.Where(s => s.CourseId == courseId);
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                lessons = lessons.Where(s => s.Title!.Contains(searchString)
                || s.Introduction!.Contains(searchString));
            }

            lessons = sortOrder switch
            {
                "title_desc" => lessons.OrderByDescending(s => s.Title),
                "date_created" => lessons.OrderBy(s => s.DateCreated),
                "date_created_desc" => lessons.OrderByDescending(s => s.DateCreated),
                _ => lessons.OrderBy(s => s.Title),
            };

            return PaginatedList<LessonViewModel>.Create(_mapper.Map<IEnumerable<LessonViewModel>>(await lessons.ToListAsync()), pageNumber ?? 1, pageSize);
        }

        public async Task<LessonViewModel> GetById(int id)
        {
            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(m => m.Id == id);
            return _mapper.Map<LessonViewModel>(lesson);
        }

        public async Task<int> Update(LessonViewModel request)
        {
            if (!LessonExists(request.Id))
            {
                throw new Exception("Lesson does not exist");
            }
            _context.Update(_mapper.Map<Lesson>(request));
            return await _context.SaveChangesAsync();
        }

        private bool LessonExists(int id)
        {
            return _context.Lessons.Any(e => e.Id == id);
        }
    }
}