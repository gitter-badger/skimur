﻿using System.Linq;
using System.Web.Mvc;
using Infrastructure;
using Infrastructure.Messaging;
using Skimur.Web.Models;
using Subs;
using Subs.Commands;
using Subs.ReadModel;

namespace Skimur.Web.Controllers
{
    public class SubsController : Controller
    {
        private readonly IContextService _contextService;
        private readonly ISubDao _subDao;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;
        private readonly IUserContext _userContext;

        public SubsController(IContextService contextService,
            ISubDao subDao,
            IMapper mapper,
            ICommandBus commandBus,
            IUserContext userContext)
        {
            _contextService = contextService;
            _subDao = subDao;
            _mapper = mapper;
            _commandBus = commandBus;
            _userContext = userContext;
        }

        public ActionResult Index(string query)
        {
            var subscribedSubs = _contextService.GetSubscribedSubNames();
            var allSubs = _subDao.GetAllSubs(query).Select(x =>
            {
                var model = _mapper.Map<Sub, SubModel>(x);

                if (subscribedSubs.Contains(model.Name))
                    model.IsSubscribed = true;

                return model;
            }).ToList();

            ViewBag.Query = query;

            return View(allSubs);
        }

        public ActionResult Posts(string name)
        {
            if (string.IsNullOrEmpty(name))
                return Redirect(Url.Subs());

            var sub = _subDao.GetSubByName(name);

            if (sub == null)
                return Redirect(Url.Subs(name));

            var model = new SubPosts();
            model.Sub = _mapper.Map<Sub, SubModel>(sub);
            model.Sub.IsSubscribed = _contextService.IsSubcribedToSub(sub.Name);

            return View(model);
        }

        public ActionResult PostsAll()
        {
            var model = new SubPosts();
            return View(model);
        }

        public ActionResult Random()
        {
            var randomSub = _subDao.GetRandomSub();

            if (randomSub == null)
                return Redirect(Url.Subs());

            return Redirect(Url.Sub(randomSub.Name));
        }

        public ActionResult Edit(string id)
        {
            var name = id;

            if (string.IsNullOrEmpty(name))
                return Redirect(Url.Subs());

            var sub = _subDao.GetSubByName(name);

            if (sub == null)
                return Redirect(Url.Subs(name));

            var model = _mapper.Map<Sub, CreateEditSubModel>(sub);
            model.IsEditing = true;

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, CreateEditSubModel model)
        {
            var name = id;
            model.IsEditing = true;
            model.Name = name;

            var response = _commandBus.Send<EditSub, EditSubResponse>(new EditSub
            {
                EditedByUserId = _userContext.CurrentUser.Id,
                Name = name,
                Description = model.Description,
                SidebarText = model.SidebarText
            });

            if (!string.IsNullOrEmpty(response.Error))
            {
                ModelState.AddModelError(string.Empty, response.Error);
                return View(model);
            }

            // todo: success message


            return View(model);
        }

        [Authorize]
        public ActionResult Create()
        {
            return View(new CreateEditSubModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateEditSubModel model)
        {
            var response = _commandBus.Send<CreateSub, CreateSubResponse>(new CreateSub
            {
                CreatedByUserId = _userContext.CurrentUser.Id,
                Name = model.Name,
                Description = model.Description,
                SidebarText = model.SidebarText
            });

            if (!string.IsNullOrEmpty(response.Error))
            {
                ModelState.AddModelError(string.Empty, response.Error);
                return View(model);
            }

            // todo: success message

            return Redirect(Url.EditSub(response.SubName));
        }

        [Authorize]
        public ActionResult CreatePost()
        {
            return View(new CreatePostViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePost(CreatePostViewModel model)
        {
            var response = _commandBus.Send<CreatePost, CreatePostResponse>(new CreatePost
            {
                CreatedByUserId = _userContext.CurrentUser.Id,
                IpAddress = Request.UserHostAddress,
                Title = model.Title,
                Url = model.Url,
                Content = model.Content,
                PostType = model.PostType,
                SubName = model.SubName,
                NotifyReplies = model.NotifyReplies
            });

            if (!string.IsNullOrEmpty(response.Error))
            {
                ModelState.AddModelError(string.Empty, response.Error);
                return View(model);
            }

            // todo: success message

            return Redirect(Url.Post(response.Slug, response.Title));
        }

        public ActionResult SideBar()
        {
            var allSubs = _subDao.GetSubByNames(_contextService.GetSubscribedSubNames()).Select(x =>
            {
                var model = _mapper.Map<Sub, SubModel>(x);
                model.IsSubscribed = true;
                return model;
            }).ToList();

            return PartialView("_SideBar", allSubs);
        }

        public ActionResult TopBar()
        {
            var allSubs = _subDao.GetSubByNames(_contextService.GetSubscribedSubNames()).Select(x =>
            {
                var model = _mapper.Map<Sub, SubModel>(x);
                model.IsSubscribed = true;
                return model;
            }).ToList();

            return PartialView("_TopBar", allSubs);
        }
    }
}