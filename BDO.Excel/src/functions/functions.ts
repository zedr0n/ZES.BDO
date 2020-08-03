import {Mutation, Query, SingleQuery} from './queries';

/**
 * @customfunction
 * @param invocation Custom function handler
 */
export function activeBranch(invocation : CustomFunctions.StreamingInvocation<string>) : void {
  const query = `query { activeBranch }`;

  Query(query, data => data.activeBranch.toString(), invocation);
}

/**
 * @customfunction
 * @param {string} name Item name
 * @param {number} grade Item grade. If omitted, grade = 0
 */
export async function itemId(name : string, grade? : number) : Promise<any> {
  if (grade == null)
    grade = 0;
  
  const query = `query {
    itemInfo(name : "${name}", grade : ${grade}) { itemId } 
  }`;

  let id = 0;
  let res = await SingleQuery(query, data => data.itemInfo.itemId.toString());
  try {
    id = Number(res);
  }
  catch (e) {
    return res;
  }

  if (id == 0)
  {
    const mutation = `mutation {
        addItem( name : "${name}", grade : ${grade} )
      }`;
    await Mutation(mutation);
  }
  id = Number(await SingleQuery(query, data => data.itemInfo.itemId.toString()));
  return id;
}

