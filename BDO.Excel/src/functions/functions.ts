import { graphQlQuery } from './queries';
import { request } from 'graphql-request';

var server : string = "https://localhost:5001";
// @ts-ignore
//window.branch = "";

/**
 * @customfunction
 * @param url Server url
 * @param period Update period
 * @param invocation Custom function handler
 */
export function activeBranch(url : string, period : number, invocation : CustomFunctions.StreamingInvocation<string>) : void {
  const query = `query { activeBranch }`;

  graphQlQuery(url, query, data => data.activeBranch.toString(), period, invocation);
}

/**
 * Gets the star count for a given Github repository.
 * @customfunction
 * @param {string} userName string name of Github user or organization.
 * @param {string} repoName string name of the Github repository.
 * @return {number} number of stars given to a Github repository.
 */
export async function getStarCount(userName, repoName) {
  try {
    //You can change this URL to any web request you want to work with.
    const url = "https://api.github.com/repos/" + userName + "/" + repoName;
    const response = await fetch(url);
    //Expect that status code is in 200-299 range
    if (!response.ok) {
      throw new Error(response.statusText)
    }
    const jsonResponse = await response.json();
    return jsonResponse.watchers_count;
  }
  catch (error) {
    return error;
  }
}
