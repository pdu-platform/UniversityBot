using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.EntityFrameworkCore;
using UniversityBot.Blazor.Data;
using UniversityBot.Blazor.Extension;
using UniversityBot.Core.DAL;
using UniversityBot.Core.Util;
using UniversityBot.EF;
using UniversityBot.Infrastructure;
using static UniversityBot.Blazor.Extension.ConverterFactory;

namespace UniversityBot.Blazor.Pages.Questioner
{
    public partial class Questioner
    {
        private Tree _tree;

        private UnitOfWork _uow;

        private TreeNode SelectNode
        {
            get
            {
                if (_questions == null || _questions.Count == 0)
                    return null;

                return _tree?.SelectedNodes.FirstOrDefault();
            }
        }

        private string InputQuestion { get; set; }

        private List<QuestionDto> _questions;
        private Dictionary<Guid, List<QuestionDto>> _questionsTree;
        private Dictionary<Guid, bool> _hasChild;

        private static QuestionDto AsEntity(TreeNode node) => (QuestionDto) node?.DataItem;

        private static string GetTitle(QuestionDto q) => q.Question;

        protected override async Task OnInitializedAsync()
        {
            _uow = new UnitOfWork(Context);
            _questions = await _uow.QuestionRepository
                .Query()
                .AsNoTracking()
                .Where(e => e.ParentId == null)
                .Project(To<QuestionDto>())
                .ToListAsync();

            _hasChild = new Dictionary<Guid, bool>();
            _questionsTree = new Dictionary<Guid, List<QuestionDto>>();
        }

        private List<QuestionDto> GetAllChild(QuestionDto question)
        {
            return _questionsTree.GetOrAdd(question.Id, id =>
            {
                return _uow.QuestionRepository
                    .Query()
                    .AsNoTracking()
                    .Where(e => e.ParentId == id)
                    .Project(To<QuestionDto>())
                    .ToList();
            });
        }

        private bool HasChild(QuestionDto question)
            => _hasChild.GetOrAdd(question.Id, id => _uow.QuestionRepository.Query().Any(e => e.ParentId == id));

        private async Task AddSon()
        {
            var selectNode = SelectNode;
            if (selectNode == null || string.IsNullOrWhiteSpace(InputQuestion))
                return;

            var selectQuestion = AsEntity(selectNode);

            var insertEntity = CreateQuestionFromInputData(selectQuestion.Id);

            await using (_uow.AsyncSaveScope())
                await _uow.QuestionRepository.AddAsync(insertEntity);

            var child = _questionsTree.GetOrAdd(selectQuestion.Id, _ => new List<QuestionDto>());

            var dto = insertEntity.Map(To<QuestionDto>());
            child.Add(dto);

            _hasChild[selectQuestion.Id] = true;

            InputQuestion = string.Empty;
            StateHasChanged();
        }

        private async Task AddParent()
        {
            var selectNode = SelectNode;
            if (selectNode == null || string.IsNullOrWhiteSpace(InputQuestion))
                return;

            var currentQuestion = AsEntity(selectNode);

            var parent = AsEntity(selectNode.ParentNode);

            var insertEntity = CreateQuestionFromInputData(parent?.Id);

            await using (_uow.AsyncSaveScope())
            {
                await _uow.QuestionRepository.AddAsync(insertEntity);

                var selectDbQuestion = await _uow.QuestionRepository.FindAsync(currentQuestion.Id);
                selectDbQuestion.ChangeParent(insertEntity);
                _uow.QuestionRepository.Update(selectDbQuestion);
            }

            var newDtoQuestion = insertEntity.Map(To<QuestionDto>());
            if (parent == null)
            {
                _questions.Remove(currentQuestion);
                _questions.Add(newDtoQuestion);
            }
            else
            {
                if (_questionsTree.TryGetValue(parent.Id, out var child))
                {
                    child.Add(newDtoQuestion);

                    child.Remove(currentQuestion);
                }
                // невозможно
                else
                {
                    _questionsTree.Add(parent.Id, new List<QuestionDto>
                    {
                        newDtoQuestion
                    });
                }
            }

            _questionsTree.Add(newDtoQuestion.Id, new List<QuestionDto>
            {
                currentQuestion
            });
            _hasChild.Add(newDtoQuestion.Id, true);

            InputQuestion = string.Empty;
            StateHasChanged();
        }

        private async Task DeleteWithChild()
        {
            var selectNode = SelectNode;
            if (selectNode == null)
                return;

            var selectQuestion = AsEntity(selectNode);

            await using (_uow.AsyncSaveScope())
            {
                await _uow.QuestionRepository.DeleteWithChildAsync(selectQuestion.Id);
            }

            var parentId = AsEntity(selectNode.ParentNode)?.Id;

            if (selectNode.ParentNode == null)
            {
                _questions.Remove(selectQuestion);
            }

            RemoveWithAllChild(selectQuestion.Id, parentId);

            StateHasChanged();
        }

        private async Task DeleteWithoutChild()
        {
            var selectNode = SelectNode;
            if (selectNode == null)
                return;

            var selectQuestion = AsEntity(selectNode);

            await using (_uow.AsyncSaveScope())
            {
                await _uow.QuestionRepository.DeleteWithoutChildAsync(selectQuestion.Id);
            }

            var parentId = AsEntity(selectNode.ParentNode)?.Id;

            if (selectNode.ParentNode == null)
            {
                _questions.Remove(selectQuestion);
            }

            RemoveWithoutAllChild(selectQuestion.Id, parentId);

            StateHasChanged();
        }

        private void Edit()
        {
            var selectNode = SelectNode;
            if (selectNode == null)
                return;

            var selectQuestion = AsEntity(selectNode);

            NavigationManager.NavigateTo($"/question/edit/{selectQuestion.Id}");
        }

        private async Task AddRoot()
        {
            if (string.IsNullOrWhiteSpace(InputQuestion))
                return;

            var insertEntity = CreateQuestionFromInputData(null);
            await using (_uow.AsyncSaveScope())
            {
                await _uow.QuestionRepository.AddAsync(insertEntity);

                var mapEntity = insertEntity.Map(To<QuestionDto>());
                _questions.Add(mapEntity);
            }

            InputQuestion = string.Empty;
            StateHasChanged();
        }

        private void RemoveWithoutAllChild(Guid id, Guid? parentId)
        {
            if (_questionsTree.TryGetValue(id, out var child))
            {
                if (parentId == null)
                {
                    _questions.AddRange(child);
                }
                else
                {
                    if (_questionsTree.TryGetValue(id, out var parentChild))
                    {
                        parentChild.AddRange(child);
                    }
                    else
                    {
                        _questionsTree.Add(id, child);
                    }
                }
            }

            RemoveFromParent(id, parentId);
        }

        private void RemoveWithAllChild(Guid id, Guid? parentId)
        {
            if (_questionsTree.TryGetValue(id, out var child))
            {
                _hasChild.Remove(id);
                foreach (var questionDto in child)
                    RemoveWithAllChild(questionDto.Id, null);
            }

            _questionsTree.Remove(id);

            RemoveFromParent(id, parentId);
        }

        private void RemoveFromParent(Guid id, Guid? parentId)
        {
            if (parentId == null || !_questionsTree.TryGetValue(parentId.Value, out var child))
                return;

            var unwrapParentId = parentId.Value;

            child.RemoveFirst(dto => dto.Id == id);
            if (child.Count != 0)
                return;

            _questionsTree.Remove(unwrapParentId);
            _hasChild.Remove(unwrapParentId);
        }

        private BotQuestioner CreateQuestionFromInputData(Guid? parent)
        {
            return new(Guid.Empty, null, InputQuestion, string.Empty)
            {
                ParentId = parent
            };
        }
    }
}