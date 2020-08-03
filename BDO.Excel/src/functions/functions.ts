import { Query } from './queries';

/**
 * @customfunction
 * @param url Server url
 * @param period Update period
 * @param invocation Custom function handler
 */
export function activeBranch(period : number, invocation : CustomFunctions.StreamingInvocation<string>) : void {
  const query = `query { activeBranch }`;

  Query(query, data => data.activeBranch.toString(), period, invocation);
}

