using Ayura.API.Features.Community.DTOs;
using Ayura.API.Services;
using Microsoft.AspNetCore.Mvc;
using CommunityModel = Ayura.API.Models.Community; // Create an alias for the type
using PostModel = Ayura.API.Models.Post; // Create an alias for the type
using CommentModel = Ayura.API.Models.Comment; // Create an alias for the type

namespace Ayura.API.Features.Community.Controllers;

[ApiController]
[Route("api/communities")]
public class CommunitiesController : Controller
{
    private readonly CommentService _commentService;
    private readonly CommunityService _communityService; // Getting Service
    private readonly PostService _postService;

    // Community Service is injected on to the controller
    // This is Constructor as an arrow function
    public CommunitiesController(CommunityService communityService, PostService postService,
        CommentService commentService)
    {
        _communityService = communityService;
        _postService = postService;
        _commentService = commentService;
    }

// From here onwards methods
    // 1. GET ALL PUBLIC Communities which are not joined by the user
    [HttpGet("public/{userId:length(24)}")]
    public async Task<IActionResult> GetPublic(string userId)
    {

        var allCommunities = await _communityService.GetPublicCommunities(userId);
        // Filter out communities that the user has joined
        if (allCommunities.Any())
        {
            return Ok(allCommunities);
        }

        return NotFound(new { Message = "No Communities Found" });
    }

    // 2. GET a community by ID 
    [HttpGet("{communityId:length(24)}")] // Constraint to check whether it has 24 chars
    public async Task<IActionResult> Get(string communityId)
    {
        var existingCommunity = await _communityService.GetCommunityById(communityId);
        if (existingCommunity is null) return NotFound(new { Message = "Community not found." });

        return Ok(existingCommunity);
    }

    // 3.  Get user joined communities 
    [HttpGet("joined/{userId:length(24)}")] // Constraint to check whether it has 24 chars
    public async Task<IActionResult> GetJoinedCommunities(string userId)
    {
        var joinedCommunities = await _communityService.GetJoinedCommunities(userId);
        if (joinedCommunities.Any())
        {
            return Ok(joinedCommunities);
        }

        // Handle the case when the user is not found or has no joined communities.
        return NotFound(new { Message = "No Joined Communities Found" });
    }

    // 4. Create a Community
    [HttpPost]
    public async Task<IActionResult> CreateCommunity(CommunityModel community)
    {
        try
        {
            var createdCommunity = await _communityService.CreateCommunity(community);
            return Ok(createdCommunity);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, "An error occurred");
        }
    }

    // 5. Update a Community
    [HttpPut("{communityId:length(24)}")]
    public async Task<IActionResult> UpdateCommunity(string communityId, CommunityModel updatedCommunity)
    {
        try
        {
            //Get the community from DB
            var existingCommunity = await _communityService.GetCommunityById(communityId);

            if (existingCommunity is null) return NotFound(new { Message = "Community not found." });

            updatedCommunity.Id = existingCommunity.Id;
            // Since this call Company Model the MembersList will be an empty String

            // Preserve old MembersList
            updatedCommunity.Members = existingCommunity.Members;
            await _communityService.UpdateCommunity(updatedCommunity);

            // Create the response object
            var response = new
            {
                Message = "Community updated successfully.",
                Community = updatedCommunity // Include the updated community
            };

            return Ok(updatedCommunity);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, new { Message = "An error occurred." });
        }
    }

    // 6. Delete a community
    [HttpDelete("{communityId:length(24)}")]
    public async Task<IActionResult> Delete(string communityId)
    {
        try
        {
            var existingCommunity = await _communityService.GetCommunityById(communityId);

            if (existingCommunity is null) return NotFound(new { Message = "Community not found." });

            await _communityService.DeleteCommunity(existingCommunity);
            return Ok(new
            {
                Message = "Community deleted successfully."
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, new { Message = "An error occurred." });
        }
    }

    // 7. Adding a member to Community
    [HttpPut("addMember")]
    public async Task<IActionResult> AddMember([FromBody] MemberRequest memberRequest)
    {
        try
        {
            // Get user by email
            var user = await _communityService.GetUserByEmail(memberRequest.UserEmail);

            if (user.Id == null)
                return NotFound(new
                {
                    Message = "User Email Not Found"
                });

            var community = await _communityService.AddMember(memberRequest.CommunityId, user.Id);

            return community.Id == null
                ? NotFound(new { Message = "Member is already added" })
                : Ok(new { Message = "Member Added successfully.", Community = community });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, new { Message = "An error occurred." });
        }
    }

    // 8. Get all posts of a community
    [HttpGet("posts/{communityId:length(24)}")]
    public async Task<IActionResult> GetCommunityPosts(string communityId)
    {
        try
        {
            var existingCommunity = await _communityService.GetCommunityById(communityId);
            if (existingCommunity is null) return NotFound(new { Message = "Community not found." });

            var allPosts = await _postService.GetPosts(communityId);
            return Ok(allPosts);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            // Log the exception and return an appropriate error response
            return StatusCode(500, new { Message = "An error occurred while processing your request." });
        }
    }

    // 9. GET a post by ID
    [HttpGet("post/{id:length(24)}")] // Constraint to check whether it has 24 chars
    public async Task<IActionResult> GetPost(string id)
    {
        var existingPost = await _postService.GetPost(id);
        if (existingPost is null) return NotFound(new { Message = "Post not found." });

        return Ok(existingPost);
    }

    // 10.Adding a post to community
    [HttpPost("post")]
    public async Task<IActionResult> CreatePost(PostModel post)
    {
        try
        {
            var existingCommunity = await _communityService.GetCommunityById(post.CommunityId);
            if (existingCommunity is null) return NotFound(new { Message = "Community not found." });

            var createdPost = await _postService.CreatePost(post);
            return CreatedAtAction("Get", new { id = createdPost.Id }, createdPost);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            // Log the exception and return an appropriate error response
            return StatusCode(500, new { Message = "An error occurred while processing your request." });
        }
    }

    // 11.Edit post
    [HttpPut("post/{id:length(24)}")]
    public async Task<IActionResult> UpdatePost(string id, PostModel updatedPost)
    {
        //Get the post from DB
        var existingPost = await _postService.GetPost(id);

        if (existingPost is null) return NotFound(new { Message = "Post not found." });

        updatedPost.Id = existingPost.Id;
        // Since this call Company Model the CommentList will be an empty String
        // Preserve old MembersList
        updatedPost.Comments = existingPost.Comments;
        await _postService.UpdatePost(updatedPost);

        // Create the response object
        var response = new
        {
            Message = "Post updated successfully.",
            Post = updatedPost // Include the updated community
        };

        return Ok(response);
    }

    // 12. delete post 
    [HttpDelete("post/{id:length(24)}")]
    public async Task<IActionResult> DeletePost(string id)
    {
        try
        {
            var existingPost = await _postService.GetPost(id);

            if (existingPost == null) return NotFound("Post not found.");

            await _postService.DeletePost(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred");
        }
    }

    // 13. create comment
    [HttpPost("comment")]
    public async Task<IActionResult> AddComment(CommentModel comment)
    {
        var createdComment = await _commentService.CreateComment(comment);
        return CreatedAtAction("Get", new { id = createdComment.Id }, createdComment);
    }

    // // 14. edit comment
    // [HttpPut("comment")]
    // public async Task<IActionResult> EditComment(string commentContent, string commentId)
    // {
    //     try
    //     {
    //         await _commentService.UpdateComment(commentContent, commentId);
    //         return Ok("Comment updated successfully.");
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(500, $"An error occurred: {ex.Message}");
    //     }
    // }

    // 15. delete comment 
    [HttpDelete("comment/{id:length(24)}")]
    public async Task<IActionResult> DeleteComment(string id)
    {
        try
        {
            var existingComment = await _commentService.GetComment(id);

            if (existingComment == null) return NotFound("Comment not found.");

            await _commentService.DeleteComment(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred");
        }
    }


    // 16. Get Community Members
    [HttpGet("getMembers/{communityId:length(24)}")]
    public async Task<IActionResult> GetCommunityMembers(string communityId)
    {
        try
        {
            var users = await _communityService.GetCommunityMembers(communityId);

            if (users.Any()) return Ok(users);

            return NotFound(new
            {
                Message = "No Members Found"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, "An error occurred");
        }
    }
}