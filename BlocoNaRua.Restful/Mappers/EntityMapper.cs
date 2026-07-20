using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Models.CarnivalBlock;
using BlocoNaRua.Restful.Models.CarnivalBlockMember;
using BlocoNaRua.Restful.Models.Meeting;
using BlocoNaRua.Restful.Models.MeetingPresence;
using BlocoNaRua.Restful.Models.Member;

namespace BlocoNaRua.Restful.Mappers;

public static class EntityMapper
{
    public static MemberResponse ToDTO(this MemberEntity entity) =>
        new(entity.Id, entity.Name, entity.Email, entity.Phone, entity.ProfileImage, entity.Uuid, entity.CreatedAt, entity.UpdatedAt);

    public static MeetingResponse ToDTO(this MeetingEntity entity) =>
        new(entity.Id, entity.Name, entity.Description, entity.Location, entity.MeetingCode, entity.MeetingDateTime, entity.CarnivalBlockId, entity.CreatedAt, entity.UpdatedAt);

    public static CarnivalBlockResponse ToDTO(this CarnivalBlockEntity entity) =>
        new(entity.Id, entity.OwnerId, entity.Name, entity.InviteCode, entity.ManagersInviteCode, entity.CarnivalBlockImage, entity.CreatedAt, entity.UpdatedAt);

    public static CarnivalBlockMemberResponse ToDTO(this CarnivalBlockMembersEntity entity) =>
        new(entity.Id, entity.CarnivalBlockId, entity.MemberId, entity.Role, entity.CreatedAt.GetValueOrDefault(), entity.UpdatedAt.GetValueOrDefault());

    public static MeetingPresenceResponse ToDTO(this MeetingPresenceEntity entity) =>
        new(entity.Id, entity.MemberId, entity.MeetingId, entity.CarnivalBlockId, entity.IsPresent, entity.CreatedAt, entity.UpdatedAt);
}
